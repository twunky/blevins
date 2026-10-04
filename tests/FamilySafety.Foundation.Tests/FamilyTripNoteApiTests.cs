using System.Net;
using System.Net.Http.Json;
using FamilySafety.Application.Profiles;
using FamilySafety.Application.Trips;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace FamilySafety.Foundation.Tests;

public sealed class FamilyTripNoteApiTests : IClassFixture<FamilyTripNoteApiTests.NoteApiFactory>
{
    private readonly HttpClient client;
    private readonly StubNoteService service;

    public FamilyTripNoteApiTests(NoteApiFactory factory)
    {
        client = factory.CreateClient();
        service = factory.Service;
        service.Reset();
    }

    [Fact]
    public async Task RoutesBindOnlyContentAndKeepModerationSeparate()
    {
        var tripId = Guid.NewGuid();
        var noteId = Guid.NewGuid();
        var root = $"/api/trips/{tripId:D}/notes";
        var list = await client.GetFromJsonAsync<List<TripNoteView>>(root);
        Assert.Empty(list!);
        var added = await client.PostAsJsonAsync(root, new
        {
            content = "Flight booked", authorProfileId = Guid.NewGuid(),
            state = "CreatorHidden", createdAt = DateTimeOffset.MinValue,
        });
        Assert.Equal(HttpStatusCode.Created, added.StatusCode);
        Assert.Equal("Flight booked", service.LastContent);
        Assert.Equal("Content", Assert.Single(typeof(TripNoteContentRequest).GetProperties()).Name);
        Assert.Equal(HttpStatusCode.OK, (await client.PutAsJsonAsync($"{root}/{noteId:D}",
            new { content = "Changed", authorProfileId = Guid.NewGuid() })).StatusCode);
        Assert.Equal(noteId, service.LastNoteId);
        Assert.Equal(HttpStatusCode.NoContent, (await client.DeleteAsync($"{root}/{noteId:D}")).StatusCode);
        Assert.Equal(HttpStatusCode.NoContent,
            (await client.PostAsJsonAsync($"{root}/{noteId:D}/hide", new { content = "Cannot rewrite" })).StatusCode);
        Assert.Equal("hide", service.LastAction);
        Assert.Equal(HttpStatusCode.MethodNotAllowed, (await client.GetAsync($"{root}/{noteId:D}")).StatusCode);
    }

    [Fact]
    public async Task UnauthenticatedUnavailableAndInvalidUseEstablishedResponses()
    {
        var root = $"/api/trips/{Guid.NewGuid():D}/notes";
        service.Unauthenticated = true;
        Assert.Equal(HttpStatusCode.Unauthorized, (await client.GetAsync(root)).StatusCode);
        service.Unauthenticated = false;
        service.Unavailable = true;
        Assert.Equal(HttpStatusCode.NotFound, (await client.GetAsync(root)).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await client.PostAsJsonAsync(root, new { content = "X" })).StatusCode);
        service.Unavailable = false;
        service.Invalid = true;
        var response = await client.PostAsJsonAsync(root, new { content = " " });
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Contains("Content", await response.Content.ReadAsStringAsync());
    }

    public sealed class NoteApiFactory : WebApplicationFactory<Program>
    {
        public StubNoteService Service { get; } = new();
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Testing");
            builder.UseSetting("ConnectionStrings:FamilySafety", "Server=localhost;Database=Unused;Trusted_Connection=True;TrustServerCertificate=True");
            builder.UseSetting("DevelopmentIdentity:Enabled", "false");
            builder.UseSetting("DevelopmentFixtures:Enabled", "false");
            builder.UseSetting("DevelopmentInvitationOutbox:Enabled", "false");
            builder.ConfigureTestServices(services =>
            {
                services.RemoveAll<IFamilyTripNoteService>();
                services.AddSingleton<IFamilyTripNoteService>(Service);
            });
        }
    }

    public sealed class StubNoteService : IFamilyTripNoteService
    {
        public string? LastContent { get; private set; }
        public Guid LastNoteId { get; private set; }
        public string? LastAction { get; private set; }
        public bool Unauthenticated { get; set; }
        public bool Unavailable { get; set; }
        public bool Invalid { get; set; }
        public void Reset()
        {
            LastContent = null; LastNoteId = Guid.Empty; LastAction = null;
            Unauthenticated = false; Unavailable = false; Invalid = false;
        }
        public Task<IReadOnlyList<TripNoteView>> ListAsync(Guid tripId, CancellationToken cancellationToken)
        {
            Check();
            return Task.FromResult<IReadOnlyList<TripNoteView>>([]);
        }
        public Task<TripNoteView> AddAsync(Guid tripId, TripNoteContentRequest request, CancellationToken cancellationToken)
        {
            Check(); LastContent = request.Content;
            return Task.FromResult(View());
        }
        public Task<TripNoteView> EditAsync(Guid tripId, Guid noteId, TripNoteContentRequest request, CancellationToken cancellationToken)
        {
            Check(); LastNoteId = noteId; LastContent = request.Content;
            return Task.FromResult(View());
        }
        public Task DeleteAsync(Guid tripId, Guid noteId, CancellationToken cancellationToken)
        {
            Check(); LastAction = "delete"; return Task.CompletedTask;
        }
        public Task HideAsync(Guid tripId, Guid noteId, CancellationToken cancellationToken)
        {
            Check(); LastAction = "hide"; return Task.CompletedTask;
        }
        private void Check()
        {
            if (Unauthenticated) throw new CurrentAccountUnavailableException();
            if (Unavailable) throw new FamilyTripNoteUnavailableException();
            if (Invalid) throw new FamilyTripNoteValidationException();
        }
        private static TripNoteView View() => new(Guid.NewGuid(), Guid.NewGuid(), "Author",
            "Content", DateTimeOffset.UtcNow, null, true, true, false);
    }
}
