describe("chat Ou", () => {
  beforeEach(() => {
    cy.login();
    cy.contains("Chat Tools").click();
    cy.get('[data-testid="sidebar-chat-ou"]').click();
  });

  it("sends a message and displays it", () => {
    cy.contains(/smart chat/i).should("exist");
    cy.get('[data-testid="chat-ou-input"]').as("msgInput");
    cy.get("@msgInput").type("Message OU");
    cy.get('[data-testid="chat-ou-send"]').click();
    cy.contains("Message OU").should("be.visible");
  });
});
