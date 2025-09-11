/// <reference types="cypress" />

interface TestUser {
  email: string;
  password: string;
  userId?: string;
}

let testUser: TestUser | null = null;

function userExistsInList($body: JQuery<HTMLElement>, email: string): boolean {
  return Array.from($body.find('[data-testid="users-email"]')).some(
    (el) => el.textContent?.trim() === email,
  );
}

// Optional type augmentation (kept minimal to avoid conflicts)
declare global {
  namespace Cypress {
    interface Chainable {
      login(): Chainable<void>;
      loginAdmin(): Chainable<void>;
    }
  }
}

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

// Admin login (exact same logic as login(), but targets port 4002 and admin credentials)
Cypress.Commands.add("loginAdmin", () => {
  const appUrl = "https://localhost:4002";
  const email =
    (Cypress.env("ADMIN_EMAIL") as string) ||
    (Cypress.env("TEACHER_EMAIL") as string) ||
    "admin_fixed_user@example.com";
  const password =
    (Cypress.env("ADMIN_PASSWORD") as string) ||
    (Cypress.env("TEACHER_PASSWORD") as string) ||
    "Passw0rd!";

  if (!testUser || testUser.email !== email) {
    testUser = { email, password };
  }
  Cypress.env("e2eAdminUser", testUser);

  cy.log(`[e2e] Admin login (or signup) deterministic user ${email}`);
  cy.intercept("POST", "**/auth/login").as("loginRequest");
  cy.intercept("POST", "**/user").as("createUser");

  cy.visit(appUrl + "/");
  cy.get('[data-testid="auth-tab-login"]').click();
  cy.get('[data-testid="auth-email"]').clear().type(email);
  cy.get('[data-testid="auth-password"]').clear().type(password);
  cy.get('[data-testid="auth-submit"]').should("not.be.disabled").click();

  cy.wait("@loginRequest").then((loginInt) => {
    const status = loginInt?.response?.statusCode;
    if (status === 200) {
      cy.log(`[e2e] Admin logged in existing deterministic user ${email}`);
      cy.get('[data-testid="ps-sidebar-container-test-id"]').should("exist");
      if (!testUser!.userId) {
        cy.window().then((win) => {
          try {
            const credsRaw = win.localStorage.getItem("credentials");
            if (credsRaw) {
              const parsed = JSON.parse(credsRaw);
              if (parsed?.userId) {
                testUser!.userId = parsed.userId;
                cy.log(`[e2e] Derived admin userId: ${parsed.userId}`);
              }
            }
          } catch {}
        });
      }
    } else {
      cy.log(`[e2e] Admin login failed ${status}; attempting signup ${email}`);
      cy.get('[data-testid="auth-tab-signup"]').click();
      cy.get('[data-testid="auth-email"]').clear().type(email);
      cy.get('[data-testid="auth-password"]').clear().type(password);
      cy.get('[data-testid="auth-confirm-password"]').clear().type(password);
      cy.get('input[placeholder*="First" i]').clear().type("Admin");
      cy.get('input[placeholder*="Last" i]').clear().type("User");
      cy.get('[data-testid="auth-submit"]').should("not.be.disabled").click();
      cy.get('[data-testid="ps-sidebar-container-test-id"]').should("exist");
      cy.wait("@createUser").then((intc) => {
        const body = intc?.response?.body as { userId?: string } | undefined;
        if (body?.userId) {
          testUser!.userId = body.userId;
          cy.log(`[e2e] Captured created admin userId: ${body.userId}`);
        } else {
          cy.log(
            "[e2e] Warning: admin userId not present in createUser response",
          );
        }
      });
    }
  });
});

export const deleteCreatedUser = () => {
  if (!testUser) {
    return cy.log("[e2e] No deterministic test user to delete.");
  }

  const { email: targetEmail, password: targetPassword } = testUser;
  const adminOrigin = "https://localhost:4002";

  cy.log(`[e2e] Deleting deterministic user '${targetEmail}'.`);

  const runDeletion = () => {
    // Ensure logged in
    cy.get("body").then(($b) => {
      const loggedIn =
        $b.find('[data-testid="ps-sidebar-container-test-id"]').length > 0;
      if (!loggedIn) {
        cy.get('[data-testid="auth-email"]', { timeout: 10000 })
          .should("exist")
          .clear()
          .type(targetEmail);
        cy.get('[data-testid="auth-password"]').clear().type(targetPassword);
        cy.get('[data-testid="auth-submit"]').should("not.be.disabled").click();
        cy.get('[data-testid="ps-sidebar-container-test-id"]', {
          timeout: 15000,
        }).should("exist");
      }
    });

    cy.get('[data-testid="sidebar-users"]', { timeout: 15000 }).click();
    cy.get("body", { timeout: 15000 }).should("exist");

    cy.get("body").then(($b) => {
      const found = userExistsInList($b, targetEmail);
      if (!found) {
        cy.log(`[e2e] User '${targetEmail}' not present; nothing to delete.`);
        return;
      }
    });

    cy.contains('[data-testid="users-email"]', targetEmail)
      .closest('tr[data-testid^="users-row-"]')
      .then(($row) => {
        if ($row.length) {
          const testId = $row.attr("data-testid");
          cy.on("window:confirm", () => true);
          cy.wrap($row)
            .find('[data-testid="users-delete-btn"]')
            .click({ force: true });
          if (testId) {
            cy.get(`[data-testid="${testId}"]`, { timeout: 10000 }).should(
              "not.exist",
            );
          }
        }
      });

    cy.contains('[data-testid="users-email"]', targetEmail).should("not.exist");
    cy.log(`[e2e] Deleted deterministic user '${targetEmail}'.`);

    cy.get("body").then(($b) => {
      const logoutBtn = $b.find('[data-testid="sidebar-logout"]');
      if (logoutBtn.length) {
        cy.wrap(logoutBtn).click();
        cy.log("[e2e] Logged out after deletion.");
      }
    });
  };

  return cy.location("origin").then((origin) => {
    if (origin === adminOrigin) {
      runDeletion();
      return cy.then(() => {
        testUser = null;
        cy.log("[e2e] Cleared cached testUser after deletion (same-origin).");
      });
    }

    return cy
      .origin(
        adminOrigin,
        { args: { targetEmail, targetPassword } },
        ({ targetEmail, targetPassword }) => {
          // inside origin context
          cy.visit("/");
          cy.get("body").then(($b) => {
            const loggedIn =
              $b.find('[data-testid="ps-sidebar-container-test-id"]').length >
              0;
            if (!loggedIn) {
              cy.get('[data-testid="auth-email"]', { timeout: 10000 })
                .should("exist")
                .clear()
                .type(targetEmail);
              cy.get('[data-testid="auth-password"]')
                .clear()
                .type(targetPassword);
              cy.get('[data-testid="auth-submit"]')
                .should("not.be.disabled")
                .click();
              cy.get('[data-testid="ps-sidebar-container-test-id"]', {
                timeout: 15000,
              }).should("exist");
            }
          });
          cy.get('[data-testid="sidebar-users"]', { timeout: 15000 }).click();
          cy.get("body", { timeout: 15000 }).should("exist");
          cy.get("body").then(($b) => {
            const found = Array.from(
              $b.find('[data-testid="users-email"]'),
            ).some((el) => el.textContent?.trim() === targetEmail);
            if (!found) {
              cy.log(
                `[e2e] User '${targetEmail}' not present; nothing to delete.`,
              );
              return;
            }
          });
          cy.contains('[data-testid="users-email"]', targetEmail)
            .closest('tr[data-testid^="users-row-"]')
            .then(($row) => {
              if ($row.length) {
                const testId = $row.attr("data-testid");
                cy.on("window:confirm", () => true);
                cy.wrap($row)
                  .find('[data-testid="users-delete-btn"]')
                  .click({ force: true });
                if (testId) {
                  cy.get(`[data-testid="${testId}"]`, {
                    timeout: 10000,
                  }).should("not.exist");
                }
              }
            });
          cy.contains('[data-testid="users-email"]', targetEmail).should(
            "not.exist",
          );
          cy.log(`[e2e] Deleted deterministic user '${targetEmail}'.`);
          cy.get("body").then(($b) => {
            const logoutBtn = $b.find('[data-testid="sidebar-logout"]');
            if (logoutBtn.length) {
              cy.wrap(logoutBtn).click();
              cy.log("[e2e] Logged out after deletion.");
            }
          });
        },
      )
      .then(() => {
        testUser = null;
        cy.log("[e2e] Cleared cached testUser after deletion (cross-origin).");
      });
  });
};
