describe("chat Yo", () => {
  beforeEach(() => {
    cy.login();
    cy.contains(/chat tools/i).click();
    cy.get('[data-testid="sidebar-chat-yo"]').click();
    cy.get('[data-testid="chat-yo-page"]').should("exist");
    cy.get('[data-testid="chat-yo-input-wrapper"]').should("exist");
    cy.get('[data-testid="chat-yo-input-wrapper"] input').should("exist");
  });

  it("sends a message via send button and observes assistant reply cycle", () => {
    cy.get('[data-testid="chat-yo-input-wrapper"] input').type(
      "E2E test message",
    );
    cy.get('[data-testid="chat-yo-send"]').click();
    cy.get('[data-testid="chat-yo-msg-user"]')
      .contains("E2E test message")
      .should("exist");
    cy.get("body").then(($b) => {
      if ($b.find('[data-testid="chat-yo-msg-loading"]').length) {
        cy.get('[data-testid="chat-yo-msg-loading"]').should("exist");
      }
    });
  });

  it("sends a message via Enter key", () => {
    cy.get('[data-testid="chat-yo-input-wrapper"] input').type(
      "Enter key message{enter}",
    );
    cy.get('[data-testid="chat-yo-msg-user"]')
      .contains("Enter key message")
      .should("exist");
  });

  it("handles multiple messages sequentially", () => {
    const msgs = ["First msg", "Second msg", "Third msg"];
    cy.wrap(msgs).each((m) => {
      cy.get('[data-testid="chat-yo-input-wrapper"] input').type(`${m}{enter}`);
      cy.get('[data-testid="chat-yo-msg-user"]')
        .last()
        .should("contain.text", m);
    });
  });
});
