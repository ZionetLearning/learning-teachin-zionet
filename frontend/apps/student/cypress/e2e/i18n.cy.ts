describe("i18n language switch", () => {
  beforeEach(() => {
    cy.login();
  });

  it("switches EN -> HE and back", () => {
    cy.contains(".ps-menu-button", /Languages|שפות/).click();
    cy.get("body").then(($b) => {
      const heItem = $b.find('[data-testid="sidebar-lang-he"]');
      if (heItem.length) {
        cy.wrap(heItem).click();
      } else {
        cy.contains(".ps-menu-button", /^HE$/).click();
      }
    });
    cy.contains("עברית");
    cy.contains("אנגלית");
    cy.contains(".ps-menu-button", /Languages|שפות/).click();
    cy.get("body").then(($b) => {
      const enItem = $b.find('[data-testid="sidebar-lang-en"]');
      if (enItem.length) {
        cy.wrap(enItem).click();
      } else {
        cy.contains(".ps-menu-button", /^EN$/).click();
      }
    });
    cy.get('[data-testid="sidebar-logout"]');
  });
});
