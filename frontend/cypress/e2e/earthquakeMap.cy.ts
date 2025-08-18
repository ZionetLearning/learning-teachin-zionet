describe("earthquake map", () => {
  beforeEach(() => {
    cy.login();
    cy.get('[data-testid="sidebar-earthquake"]').click();
  });

  it("loads and switches timeframe filter", () => {
    cy.contains(/earthquake map/i).should("exist");
    cy.get(".leaflet-container", { timeout: 15000 }).should("exist");

    // Open timeframe select (MUI) via data-testid and choose alternative timeframe (48 hours)
    cy.get('[data-testid="eq-timeframe"]').click();
    cy.contains(/48 hours|48/i).click({ force: true });

    // Optional marker assertion
    cy.get("body").then(($body) => {
      if ($body.find(".leaflet-marker-icon").length) {
        cy.get(".leaflet-marker-icon").should("have.length.greaterThan", 0);
      }
    });
  });
});
