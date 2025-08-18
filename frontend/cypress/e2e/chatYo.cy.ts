describe("chat Yo", () => {
  beforeEach(() => {
    cy.login();
    cy.contains("Chat Tools").click();
    cy.get('[data-testid="sidebar-chat-yo"]').click();
  });
  it("sends a message and shows it", () => {
    cy.get('textarea, input[placeholder*="Type a message"], input')
      .first()
      .as("msgInput");
    cy.get("@msgInput").type("E2E test message");
    cy.contains(/^↑$|Send|send/i).click({ force: true });
    cy.contains("E2E test message").should("be.visible");
  });
});
