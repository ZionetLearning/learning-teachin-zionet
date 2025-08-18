describe("avatar Da", () => {
  beforeEach(() => {
    cy.login();
    cy.contains("Avatar Tools").click();
    cy.get('[data-testid="sidebar-avatar-da"]').click();
  });

  it("shows input and allows typing text", () => {
    cy.contains(/avatar/i).should("exist");
    cy.get("textarea, input[placeholder], input").first().as("avatarInput");
    cy.get("@avatarInput").type("שלום");
    // Assert the input now contains some (non-empty) value
    cy.get("@avatarInput").should(($el) => {
      const val = ($el.val() as string) || "";
      expect(val.length).to.be.greaterThan(0);
    });
  });
});
