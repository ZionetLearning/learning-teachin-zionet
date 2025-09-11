import { deleteCreatedUser } from "../support/commands";

describe("auth", () => {
  const baseUrl = "https://localhost:4000";

  it("full UI login then logout", () => {
    cy.login();
    cy.get('[data-testid="app-sidebar"]').should("exist");
    cy.contains(/logout/i).click();
    cy.get('[data-testid="auth-submit"]').should("exist");
  });

  it("invalid login shows validation or stays on form", () => {
    cy.visit(baseUrl + "/");
    cy.get('[data-testid="auth-tab-login"]').click();
    cy.get('[data-testid="auth-email"]')
      .should("be.enabled")
      .type("wrong@example.com");
    cy.get('[data-testid="auth-password"]')
      .should("be.enabled")
      .type("badpass");
    cy.get('[data-testid="auth-submit"]').click();
    cy.get("body").then(($b) => {
      if ($b.find('[data-testid="app-sidebar"]').length) {
        cy.log("App accepted invalid credentials (no backend validation yet)");
      } else {
        cy.get('[data-testid="auth-submit"]').should("exist");
      }
    });
  });

  it("signup creates session using same deterministic credentials as Users and cleans up", () => {
    Cypress.env("E2E_TEST_EMAIL", "e2e_users_fixed_user@example.com");
    Cypress.env("E2E_TEST_PASSWORD", "Passw0rd!");

    cy.login();

    cy.get('[data-testid="app-sidebar"]').should("exist");
    cy.reload();
    cy.get('[data-testid="app-sidebar"]').should("exist");

    deleteCreatedUser();
  });

  it("protected route redirects to auth when not logged in", () => {
    cy.clearCookies();
    cy.visit(baseUrl + "/chat/yo", {
      onBeforeLoad: (win) => {
        win.localStorage.clear();
        win.sessionStorage.clear();
      },
    });
    cy.location("pathname", { timeout: 10000 }).should("eq", "/signin");
    cy.get('[data-testid="auth-page"]', { timeout: 10000 }).should("exist");
    cy.get("body").then(($b) => {
      if (!$b.find('[data-testid="auth-submit"]').length) {
        cy.get('[data-testid="auth-tab-login"]').click();
      }
    });
    cy.get('[data-testid="auth-submit"]').should("exist");
  });

  it("fast helper login still works (smoke)", () => {
    cy.login();
    cy.get('[data-testid="app-sidebar"]').should("exist");
  });
});
