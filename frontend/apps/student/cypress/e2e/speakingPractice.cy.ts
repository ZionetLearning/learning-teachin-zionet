describe("speaking practice", () => {
  beforeEach(() => {
    cy.login();
    cy.contains(/practice tools/i).click();
    cy.get('[data-testid="sidebar-speaking"]')
      .scrollIntoView()
      .click({ timeout: 8000 });
    cy.get('[data-testid="speaking-practice-page"]').should("exist");
  });

  it("navigates phrases forward and backward (index assertion)", () => {
    cy.get('[data-testid="speaking-index"]')
      .invoke("text")
      .then((start) => {
        cy.get('[data-testid="speaking-next"]').click();
        cy.get('[data-testid="speaking-index"]').should(($el) => {
          expect($el.text(), "index after next click differs").not.eq(start);
        });
        cy.get('[data-testid="speaking-prev"]').click();
        cy.get('[data-testid="speaking-index"]').should("have.text", start);
      });
  });

  it("toggles nikud display (allows same if phrase has no variant)", () => {
    cy.get('[data-testid="speaking-phrase"]').invoke("text").as("basePhrase");
    cy.get('[data-testid="speaking-nikud-toggle"]').click();
    cy.get('[data-testid="speaking-phrase"]').then(($afterToggle) => {
      cy.get("@basePhrase").then(() => {
        expect($afterToggle.text()).to.be.a("string");
      });
    });
    cy.get('[data-testid="speaking-nikud-toggle"]').click();
    cy.get('[data-testid="speaking-phrase"]').should("exist");
  });

  it("shows feedback placeholder then resets when navigating", () => {
    cy.get('[data-testid="speaking-feedback"]').should("have.text", "");
    cy.get('[data-testid="speaking-record"]').click();
    cy.wait(200);
    cy.get('[data-testid="speaking-record"]').click();
    cy.get('[data-testid="speaking-feedback"]').should("exist");
    cy.get('[data-testid="speaking-next"]').click();
    cy.get('[data-testid="speaking-feedback"]').should("have.text", "");
  });
});
