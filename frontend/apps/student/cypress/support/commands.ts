/// <reference types="cypress" />

export {};

Cypress.Commands.add(
  "login",
  (email: string = "admin@admin.com", password: string = "admin123") => {
    cy.visit("http://localhost:4000/");
    // If already authenticated skip
    cy.get("body").then(($b) => {
      if ($b.find('[data-testid="ps-sidebar-container-test-id"]').length) {
        return; // already logged in
      }
      cy.get('[data-testid="auth-email"]').clear().type(email);
      cy.get('[data-testid="auth-password"]').clear().type(password);
      cy.get('[data-testid="auth-submit"]').should("not.be.disabled").click();
      cy.get('[data-testid="ps-sidebar-container-test-id"]').should("exist");
    });
  },
);
