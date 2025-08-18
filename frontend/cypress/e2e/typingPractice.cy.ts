describe("typing practice", () => {
  beforeEach(() => {
    cy.login();
    // Ensure Practice Tools submenu is expanded so the item isn't overlapped
    cy.contains(/practice tools/i).click();
    cy.get('[data-testid="sidebar-typing"]')
      .scrollIntoView()
      .click({ timeout: 8000 });
  });

  it("selects a level, plays audio, types answer, and changes level", () => {
    // Select Easy level
    cy.contains(/easy/i).click({ force: true });
    cy.get('[data-testid="typing-change-level"]').should("be.visible");
    cy.contains(/click play to hear the hebrew text/i).should("exist");

    // Start playing (audio call is stubbed automatically in app for Cypress)
    cy.get('[data-testid="typing-play"]').click();

    // Wait for phase transition to typing (input wrapper appears)
    cy.get('[data-testid="typing-input-wrapper"]').should("exist");
    cy.get('[data-testid="typing-input"]').type("test answer");

    // Submit answer to reach feedback phase (if submit enabled)
    cy.get('[data-testid="typing-submit"]').click({ force: true });

    // Go back to change level
    cy.get('[data-testid="typing-change-level"]').click();
    cy.get('[data-testid="typing-level-selection"]').should("exist");
    cy.contains(/medium/i).click({ force: true });
    cy.get('[data-testid="typing-change-level"]').should("be.visible");
  });
});
