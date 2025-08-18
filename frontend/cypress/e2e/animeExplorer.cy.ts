describe("anime explorer", () => {
  beforeEach(() => {
    cy.login();
    cy.get('[data-testid="sidebar-anime"]').click();
    cy.contains(/Explore Anime/i).should("be.visible");
  });

  it("returns live Naruto results and handles back-to-top (no mock)", () => {
    cy.intercept({
      method: "GET",
      url: /https:\/\/api\.jikan\.moe\/v4\/anime.*q=Naruto.*/i,
    }).as("narutoSearch");

    cy.get('input[name="search"]').type("Naruto");
    cy.wait(700);
    cy.wait("@narutoSearch", { timeout: 20000 });

    cy.contains("p", /^Loading\.\.\.$/).should("not.exist");
    cy.contains(/^Naruto$/).should("be.visible");
    cy.contains(/^Type:\s*tv$/i).should("exist");
    cy.contains(/^Episodes:\s*\d+/i).should("exist");

    cy.get('button[aria-label="Back to top"]').should(
      "have.css",
      "opacity",
      "0",
    );
    cy.get('div[class*="listWrap"]').then(($el) => {
      const el = $el[0] as HTMLDivElement;
      if (el.scrollHeight <= el.clientHeight) {
        cy.log(
          "Not enough content to scroll for back-to-top visibility test (live)",
        );
        return;
      }
      el.scrollTop = Math.max(600, el.clientHeight);
      el.dispatchEvent(new Event("scroll"));
    });
    cy.get('div[class*="listWrap"]').then(($wrap) => {
      const wrap = $wrap[0] as HTMLDivElement;
      if (wrap.scrollHeight > wrap.clientHeight) {
        cy.get('button[aria-label="Back to top"]').should(
          "have.css",
          "opacity",
          "1",
        );
      } else {
        cy.log(
          "Skipped back-to-top visibility assertion due to insufficient live results",
        );
      }
    });

    cy.get('button[aria-label="Clear search"]').click();
    cy.get('input[name="search"]').should("have.value", "");
  });
});
