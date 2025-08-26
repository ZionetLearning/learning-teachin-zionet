describe("chat Ou", () => {
  beforeEach(() => {
    cy.login();
    cy.contains(/chat tools/i).click();
    cy.get('[data-testid="sidebar-chat-ou"]').click();
    cy.get('[data-testid="chat-ou-input"]').should("exist");
  });

  it("sends a message via button", () => {
    cy.contains(/smart chat/i).should("exist");
    cy.get('[data-testid="chat-ou-input"]').as("input").type("Message OU");
    cy.get('[data-testid="chat-ou-send"]').click();
    cy.contains("Message OU").should("be.visible");
  });

  it("sends a message via Enter key", () => {
    cy.get('[data-testid="chat-ou-input"]')
      .as("input")
      .type("Enter send message{enter}");
    cy.contains("Enter send message").should("be.visible");
  });

  it("clicks a suggestion to populate and send", () => {
    cy.get('[data-testid="chat-ou-input"]').focus();
    cy.get('[data-testid="chat-ou-suggestions"]').should("exist");
    cy.get('[data-testid="chat-ou-suggestion-0"]').click();
    cy.get('[data-testid="chat-ou-send"]').click();
  });
});
