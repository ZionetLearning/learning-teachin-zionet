describe("avatar Sh", () => {
  beforeEach(() => {
    cy.login();
    cy.contains(/avatar tools/i).click();
    cy.get('[data-testid="sidebar-avatar-sh"]').click();
    cy.get('[data-testid="avatar-sh-page"]').should("exist");
  });

  it("loads base UI elements", () => {
    cy.get('[data-testid="avatar-sh-avatar"]').should("be.visible");
    cy.get('[data-testid="avatar-sh-lips"]').should("be.visible");
    cy.get('[data-testid="avatar-sh-input"]').should("be.visible");
    cy.get('[data-testid="avatar-sh-speak"]').should("exist");
  });

  it("speaks entered text with simulated viseme cycle", () => {
    cy.get('[data-testid="avatar-sh-input"]')
      .as("textInput")
      .clear()
      .type("שלום עולם");
    cy.get("@textInput").should("have.value", "שלום עולם");
    cy.get('[data-testid="avatar-sh-lips"]').as("lips");
    cy.get('[data-testid="avatar-sh-speak"]').as("speakBtn").click();
    cy.get("@lips")
      .invoke("attr", "src")
      .then((initialSrc) => {
        cy.wait(350);
        cy.get("@lips")
          .invoke("attr", "src")
          .should((src) => {
            expect(src).to.not.equal(initialSrc);
          });
      });
  });
});
