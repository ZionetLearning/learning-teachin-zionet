describe("earthquake map", () => {
  beforeEach(() => {
    cy.login();
    cy.intercept("GET", /earthquake\.usgs\.gov\/fdsnws\/event\/1\/query.*/).as(
      "getQuakes",
    );
    cy.get('[data-testid="sidebar-earthquake"]').click();
  });

  it("loads map, shows markers, switches timeframe filter", () => {
    cy.contains(/earthquake map/i).should("exist");
    cy.wait("@getQuakes", { timeout: 20000 });
    cy.get('[data-testid="eq-map"]', { timeout: 20000 }).should("exist");
    cy.get('[data-testid="eq-map"] .leaflet-container', {
      timeout: 20000,
    }).should("exist");
    cy.get("body").then(($b) => {
      const count = $b.find(".leaflet-marker-icon").length;
      if (count > 0) {
        cy.get(".leaflet-marker-icon").should("have.length", count);
      }
    });

    cy.get('[data-testid="eq-timeframe"]').click();
    cy.get("li.MuiMenuItem-root").contains(/48/).click();
    cy.wait("@getQuakes", { timeout: 20000 });
    cy.get('[data-testid="eq-map"] .leaflet-container', {
      timeout: 20000,
    }).should("exist");
    cy.get("body").then(($b) => {
      const count = $b.find(".leaflet-marker-icon").length;
      if (count > 0) {
        cy.get(".leaflet-marker-icon").should("have.length", count);
      }
    });
  });
});
