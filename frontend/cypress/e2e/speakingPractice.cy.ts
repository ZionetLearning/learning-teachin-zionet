describe("speaking practice", () => {
  beforeEach(() => {
    cy.login();
    // Expand the Practice Tools submenu first to ensure the child item is not covered
    cy.contains(/practice tools/i).click();
    cy.get('[data-testid="sidebar-speaking"]')
      .scrollIntoView()
      .click({ timeout: 8000 });
  });

  it("renders speaking practice page container", () => {
    cy.contains(/practice tools/i); // side menu context present
  });
});
