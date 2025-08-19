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

  it("handles multiple sequential messages (waiting each bot completion)", () => {
    const messages = ["First", "Second", "Third"];
    cy.wrap(messages).each((m) => {
      cy.get('[data-testid="chat-da-input"]')
        .should("not.be.disabled")
        .type(String(m));
      cy.get('[data-testid="chat-da-send"]').click();
      cy.contains(String(m)).should("be.visible");
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
});
