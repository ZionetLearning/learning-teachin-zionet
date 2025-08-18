describe("word order game", () => {
  beforeEach(() => {
    cy.login();
    cy.contains("Practice Tools").click();
    cy.contains("Word Order Game").click();
    cy.contains("Word Order Game").should("be.visible");
  });

  it("interacts with word buttons (choose, reset, next)", () => {
    cy.contains(/Arrange the words/i).should("be.visible");
    cy.contains(/Loading/).should("not.exist");

    // Click first available bank word via fresh query
    cy.get('[data-testid="wog-bank"] button', { timeout: 10000 }).should(
      "have.length.greaterThan",
      0,
    );
    cy.get('[data-testid="wog-bank"] button').first().click();
    cy.get('[data-testid="wog-chosen"] button').should("have.length", 1);

    // Reset
    cy.get('[data-testid="wog-reset"]').click();
    cy.get('[data-testid="wog-chosen"] button').should("have.length", 0);

    // Click up to two words re-querying each time
    cy.get('[data-testid="wog-bank"] button')
      .its("length")
      .then((len) => {
        const n = Math.min(Number(len), 2);
        for (let i = 0; i < n; i++) {
          cy.get('[data-testid="wog-bank"] button').eq(0).click();
        }
      });
    cy.get('[data-testid="wog-chosen"] button')
      .its("length")
      .should("be.gte", 1);

    // Next sentence
    cy.get('[data-testid="wog-next"]').click();
    cy.contains(/Loading/, { timeout: 5000 }).should("not.exist");
    cy.get('[data-testid="wog-bank"] button', { timeout: 10000 }).should(
      "have.length.greaterThan",
      0,
    );
  });
});
