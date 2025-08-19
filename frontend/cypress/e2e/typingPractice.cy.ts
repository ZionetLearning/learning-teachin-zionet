describe("typing practice", () => {
  beforeEach(() => {
    cy.login();
    cy.contains(/practice tools/i).click();
    cy.get('[data-testid="sidebar-typing"]')
      .scrollIntoView()
      .click({ timeout: 8000 });
  });

  it("selects a level, plays audio, types answer, and changes level", () => {
    cy.contains(/easy/i).click({ force: true });
    cy.get('[data-testid="typing-change-level"]').should("be.visible");
    cy.contains(/click play to hear the hebrew text/i).should("exist");

    cy.get('[data-testid="typing-play"]').click();

    cy.get('[data-testid="typing-input-wrapper"]').should("exist");
    cy.get('[data-testid="typing-input"]').type("test answer");

    cy.get('[data-testid="typing-submit"]').click({ force: true });

    cy.get('[data-testid="typing-change-level"]').click();
    cy.get('[data-testid="typing-level-selection"]').should("exist");
    cy.contains(/medium/i).click({ force: true });
    cy.get('[data-testid="typing-change-level"]').should("be.visible");
  });
});
