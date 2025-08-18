describe("auth", () => {
  it("login and logout", () => {
    cy.login();
    // open sidebar logout item (label depends on i18n default EN)
    cy.contains("Logout").click();
    cy.get('[data-testid="auth-submit"]').should("exist");
  });
});
