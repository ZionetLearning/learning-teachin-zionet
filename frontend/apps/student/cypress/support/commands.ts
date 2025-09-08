/// <reference types="cypress" />

let testUser: { email: string; password: string; userId?: string } | null =
  null;

Cypress.Commands.add("login", () => {
  const appUrl = "https://localhost:4000";
  const email =
    (Cypress.env("E2E_TEST_EMAIL") as string) || "e2e_fixed_user@example.com";
  const password = (Cypress.env("E2E_TEST_PASSWORD") as string) || "Passw0rd!";

  if (!testUser || testUser.email !== email) {
    testUser = { email, password };
  }
  Cypress.env("e2eUser", testUser);

  cy.log(`[e2e] Login (or signup) with deterministic test user ${email}`);
  cy.intercept("POST", "**/auth/login").as("loginRequest");
  cy.intercept("POST", "**/user").as("createUser");

  cy.visit(appUrl + "/");
  // Try LOGIN first.
  cy.get('[data-testid="auth-tab-login"]').click();
  cy.get('[data-testid="auth-email"]').clear().type(email);
  cy.get('[data-testid="auth-password"]').clear().type(password);
  cy.get('[data-testid="auth-submit"]').should("not.be.disabled").click();

  cy.wait("@loginRequest").then((loginInt) => {
    const status = loginInt?.response?.statusCode;
    if (status === 200) {
      cy.log(`[e2e] Logged in existing deterministic user ${email}`);
      cy.get('[data-testid="ps-sidebar-container-test-id"]').should("exist");
      if (!testUser!.userId) {
        cy.window().then((win) => {
          try {
            const credsRaw = win.localStorage.getItem("credentials");
            if (credsRaw) {
              const parsed = JSON.parse(credsRaw);
              if (parsed?.userId) {
                testUser!.userId = parsed.userId;
                cy.log(
                  `[e2e] Derived userId from credentials: ${parsed.userId}`,
                );
              }
            }
          } catch {}
        });
      }
    } else {
      cy.log(
        `[e2e] Login failed with status ${status}; attempting signup for ${email}`,
      );
      cy.get('[data-testid="auth-tab-signup"]').click();
      cy.get('[data-testid="auth-email"]').clear().type(email);
      cy.get('[data-testid="auth-password"]').clear().type(password);
      cy.get('[data-testid="auth-confirm-password"]').clear().type(password);
      cy.get('input[placeholder*="First" i]').clear().type("E2E");
      cy.get('input[placeholder*="Last" i]').clear().type("User");
      cy.get('[data-testid="auth-submit"]').should("not.be.disabled").click();
      cy.get('[data-testid="ps-sidebar-container-test-id"]').should("exist");
      cy.wait("@createUser").then((intc) => {
        const body = intc?.response?.body as { userId?: string } | undefined;
        if (body?.userId) {
          testUser!.userId = body.userId;
          cy.log(`[e2e] Captured created userId: ${body.userId}`);
        } else {
          cy.log(
            "[e2e] Warning: userId not present in createUser response body during signup",
          );
        }
      });
    }
  });
});

export const deleteCreatedUser = () => {
  if (!testUser) return;
  const adminOrigin = "https://localhost:4002";
  const loginUser = testUser;
  cy.log(`[e2e] UI-deleting deterministic test user: ${loginUser.email}`);

  cy.origin(
    adminOrigin,
    { args: { email: loginUser.email, password: loginUser.password } },
    ({ email, password }) => {
      cy.visit("/");
      cy.get('[data-testid="auth-email"]', { timeout: 10000 })
        .should("exist")
        .clear()
        .type(email);
      cy.get('[data-testid="auth-password"]').clear().type(password);
      cy.get('[data-testid="auth-submit"]').should("not.be.disabled").click();
      cy.get('[data-testid="ps-sidebar-container-test-id"]', {
        timeout: 15000,
      }).should("exist");

      cy.get('[data-testid="sidebar-users"]', { timeout: 10000 })
        .should("exist")
        .click();

      cy.get('[data-testid^="users-item-"]', { timeout: 15000 }).should(
        "exist",
      );

      cy.contains('[data-testid="users-email"]', email, { timeout: 10000 })
        .scrollIntoView()
        .should("be.visible")
        .parents('li[data-testid^="users-item-"]')
        .as("targetRow");

      cy.get("@targetRow").within(() => {
        cy.get('[data-testid="users-update-btn"]').should("exist");
        cy.get('[data-testid="users-delete-btn"]').should("exist");
      });

      cy.on("window:confirm", () => true);

      cy.get("@targetRow").find('[data-testid="users-delete-btn"]').click();

      cy.contains('[data-testid="users-email"]', email).should("not.exist");

      cy.get('[data-testid="sidebar-logout"]', { timeout: 10000 })
        .should("exist")
        .click();
    },
  ).then(() => {
    testUser = null;
  });
};
