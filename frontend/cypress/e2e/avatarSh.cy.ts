describe("avatar Sh", () => {
  beforeEach(() => {
    cy.login();
    cy.contains("Avatar Tools").click();
    cy.get('[data-testid="sidebar-avatar-sh"]').click();
  });

  it("accepts Hebrew input and shows speak button", () => {
    cy.contains(/avatar/i).should("exist");
    cy.get("textarea, input[placeholder], input").first().as("avatarInput");
    cy.get("@avatarInput").type("טקסט");
    cy.get("@avatarInput").should(($el) => {
      const val = ($el.val() as string) || "";
      expect(val.length).to.be.greaterThan(0);
    });
    cy.contains(/speak|play|start/i).should("exist");
  });
});
