describe("chat Da", () => {
  beforeEach(() => {
    cy.login();
    cy.contains("Chat Tools").click();
    cy.get('[data-testid="sidebar-chat-da"]').click();
  });

  it("sends a message and displays it", () => {
    cy.contains(/chat/i).should("exist");
    cy.get('[data-testid="chat-da-input"]').as("msgInput");
    cy.get("@msgInput").type("Message DA");
    cy.get('[data-testid="chat-da-send"]').click({ force: true });
    cy.contains("Message DA").should("be.visible");
  });
});
