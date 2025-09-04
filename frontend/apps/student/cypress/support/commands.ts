/// <reference types="cypress" />

let createdUser: { email: string; password: string } | null = null;

export {};

Cypress.Commands.add("login", () => {
  const baseUrl = "http://localhost:4000";
  cy.visit(baseUrl + "/");

  cy.get("body").then(($b) => {
    if ($b.find('[data-testid="ps-sidebar-container-test-id"]').length) {
      return;
    }

    if (!createdUser) {
      const email = `e2e_user_${Date.now()}@example.com`;
      const password = "Passw0rd!";

      cy.log("[e2e] Creating new test user: " + email);
      cy.get('[data-testid="auth-tab-signup"]').click();
      cy.get('[data-testid="auth-email"]').clear().type(email);
      cy.get('[data-testid="auth-password"]').clear().type(password);
      cy.get('[data-testid="auth-confirm-password"]').clear().type(password);
      cy.get('input[placeholder*="First" i]').type("E2E");
      cy.get('input[placeholder*="Last" i]').type("User");
      cy.get('[data-testid="auth-submit"]').should("not.be.disabled").click();
      cy.get('[data-testid="ps-sidebar-container-test-id"]').should("exist");

      createdUser = { email, password };
      Cypress.env("e2eUser", createdUser);
      return;
    }

    if (!$b.find('[data-testid="auth-email"]').length) {
      if ($b.find('[data-testid="auth-tab-login"]').length) {
        cy.get('[data-testid="auth-tab-login"]').click();
      }
    }

    cy.get('[data-testid="auth-email"]').clear().type(createdUser!.email);
    cy.get('[data-testid="auth-password"]').clear().type(createdUser!.password);
    cy.get('[data-testid="auth-submit"]').should("not.be.disabled").click();
    cy.get('[data-testid="ps-sidebar-container-test-id"]').should("exist");
  });
});
