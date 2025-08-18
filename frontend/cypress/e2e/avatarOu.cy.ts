describe("avatar Ou", () => {
  beforeEach(() => {
    cy.login();
    cy.contains("Avatar Tools").click();
    cy.get('[data-testid="sidebar-avatar-ou"]').click();
  });

  it("types text and attempts speak action (button present)", () => {
    cy.contains(/avatar/i).should("exist");
    cy.get("textarea, input[placeholder], input").first().as("avatarInput");
    cy.get("@avatarInput").clear().type("Hello avatar");
    cy.contains(/speak|play|start/i).click({ force: true });
  });
});
