describe("weather page", () => {
  beforeEach(() => {
    cy.login();
    cy.get('[data-testid="sidebar-weather"]').click();
  });

  it("searches for a real city (London) and displays live weather data", () => {
    cy.get('input[name="search-city"]').clear().type("London");
    cy.contains(/search/i).click();

    cy.contains(/loading/i, { timeout: 15000 }).should("exist");
    cy.contains(/london/i, { timeout: 20000 }).should("exist");
    cy.contains(/humidity|pressure/i).should("exist");
  });

  it("attempts an unknown city and expects an error (live)", () => {
    const unknown = "NowhereVille";
    cy.get('input[name="search-city"]').clear().type(unknown);
    cy.contains(/search/i).click();
    cy.contains(/loading/i, { timeout: 15000 }).should("exist");
    cy.contains(/not found|error/i, { timeout: 20000 }).should("exist");
  });

  it("shows geolocation button", () => {
    cy.contains(/use my location/i);
  });
});
