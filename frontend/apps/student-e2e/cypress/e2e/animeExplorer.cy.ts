describe("anime explorer", () => {
  beforeEach(() => {
    cy.login();
    cy.get('[data-testid="sidebar-anime"]').click();
    cy.contains(/Explore Anime/i).should("be.visible");
  });

  it("searches live for Naruto and toggles back-to-top", () => {
    cy.intercept("GET", /https:\/\/api\.jikan\.moe\/v4\/anime.*q=Naruto.*/i).as(
      "narutoSearch",
    );

    cy.get('[data-testid="anime-search-input"]').type("Naruto");
    cy.wait("@narutoSearch", { timeout: 20000 });

    cy.contains(/^Naruto$/).should("be.visible");
    cy.contains(/^Type:\s*tv$/i).should("exist");
    cy.contains(/^Episodes:\s*\d+/i).should("exist");

    cy.get('[data-testid="anime-back-to-top"]').should(
      "have.css",
      "opacity",
      "0",
    );

    cy.get('[data-testid="anime-list"]').then(($list) => {
      const el = $list[0];
      if (el.scrollHeight > el.clientHeight) {
        cy.wrap($list).scrollTo("bottom", { ensureScrollable: false });
        cy.get('[data-testid="anime-back-to-top"]').should(
          "have.css",
          "opacity",
          "1",
        );
      } else {
        cy.log("Insufficient results to test back-to-top visibility");
      }
    });

    cy.get('[data-testid="anime-clear-search"]').click();
    cy.get('[data-testid="anime-search-input"]').should("have.value", "");
  });
});
