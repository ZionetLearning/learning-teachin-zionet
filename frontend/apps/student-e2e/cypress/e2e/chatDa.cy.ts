describe("chat Da", () => {
  beforeEach(() => {
    cy.login();
    cy.contains(/chat tools/i).click();
    cy.get('[data-testid="sidebar-chat-da"]').click();
    cy.get('[data-testid="chat-da-input"]').should("exist");
  });

  it("sends a message via button and shows user bubble then streamed bot reply", () => {
    cy.get('[data-testid="chat-da-input"]')
      .as("input")
      .should("not.be.disabled")
      .type("Hello bot");
    cy.get('[data-testid="chat-da-send"]').click();
    cy.contains("Hello bot").should("be.visible");
    cy.get('[data-testid="chat-da-msg-bot-streaming"]').should("exist");
    cy.contains(/hello,\s*this is a mock/i, { timeout: 6000 }).should(
      "be.visible",
    );
    cy.contains(/experience!/i, { timeout: 8000 }).should("be.visible");
    cy.get('[data-testid="chat-da-msg-bot-complete"]', {
      timeout: 10000,
    }).should("exist");
    cy.get('[data-testid="chat-da-input"]').then(($el) => {
      if ($el.is(":disabled")) {
        cy.wrap($el, { timeout: 10000 }).should("not.be.disabled");
      }
    });
  });

  it("sends a message via Enter key and clears input after send (waits for bot)", () => {
    cy.get('[data-testid="chat-da-input"]')
      .as("input")
      .should("not.be.disabled")
      .type("Second message{enter}");
    cy.contains("Second message").should("be.visible");
    cy.get("@input").should("have.value", "");
    cy.get('[data-testid="chat-da-msg-bot-streaming"]').should("exist");
    cy.get('[data-testid="chat-da-msg-bot-complete"]', {
      timeout: 10000,
    }).should("exist");
    cy.get('[data-testid="chat-da-input"]').then(($el) => {
      if ($el.is(":disabled")) {
        cy.wrap($el, { timeout: 10000 }).should("not.be.disabled");
      }
    });
  });
});
