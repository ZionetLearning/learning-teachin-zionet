/// <reference types="cypress" />

let usersApiBase: string | null = null;
// Accumulate all created users in a spec; we'll delete them once after all tests.
const createdUsers: Array<{
  email: string;
  password: string;
  userId?: string;
}> = [];

// Determine users API base once (no need to sniff a network call).
const computeUsersApiBase = (): string => {
  if (usersApiBase) return usersApiBase;
  const envBase =
    Cypress.env("VITE_USERS_URL") || (Cypress.env("USERS_API_BASE") as string);
  usersApiBase = envBase
    ? String(envBase).replace(/\/$/, "")
    : "https://localhost:5001/users-manager";
  Cypress.env("usersApiBase", usersApiBase);
  return usersApiBase;
};

// Refactored login: use the SAME deterministic credentials every test.
// Flow:
// 1. Attempt login with fixed credentials.
// 2. If login fails (user missing), switch to signup and create the user.
// 3. Capture userId on first creation and reuse it (stored in Cypress env & createdUsers array).
// Assumption: It's acceptable to delete this fixed user after each test; next test will recreate it.

Cypress.Commands.add("login", () => {
  const appUrl = "https://localhost:4000";
  const email =
    (Cypress.env("E2E_TEST_EMAIL") as string) || "e2e_fixed_user@example.com";
  const password = (Cypress.env("E2E_TEST_PASSWORD") as string) || "Passw0rd!";

  let record = createdUsers.find((r) => r.email === email);
  if (!record) {
    record = { email, password };
    createdUsers.length = 0;
    createdUsers.push(record);
  }
  Cypress.env("e2eUser", record);
  computeUsersApiBase();

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
      if (!record!.userId) {
        cy.window().then((win) => {
          try {
            const credsRaw = win.localStorage.getItem("credentials");
            if (credsRaw) {
              const parsed = JSON.parse(credsRaw);
              if (parsed?.userId) {
                record!.userId = parsed.userId;
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
          record!.userId = body.userId;
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

export const deleteAllCreatedUsers = () => {
  if (!createdUsers.length) return;

  const adminOrigin = "https://localhost:4002";
  const loginUser = createdUsers[0];

  const base =
    usersApiBase || Cypress.env("usersApiBase") || computeUsersApiBase();
  cy.log(`[e2e] Deleting ${createdUsers.length} test user(s)`);

  const ensureAdminLogin = () => {
    return cy
      .origin(
        adminOrigin,
        { args: { email: loginUser.email, password: loginUser.password } },
        ({ email, password }) => {
          cy.visit("/");
          cy.get('[data-testid="auth-email"]', { timeout: 10000 })
            .should("exist")
            .clear()
            .type(email);
          cy.get('[data-testid="auth-password"]').clear().type(password);
          cy.get('[data-testid="auth-submit"]')
            .should("not.be.disabled")
            .click();
          cy.get('[data-testid="ps-sidebar-container-test-id"]', {
            timeout: 15000,
          }).should("exist");
        },
      )
      .then(() => {
        cy.log(`[e2e] Admin login as test user ${loginUser.email} successful`);
      });
  };

  ensureAdminLogin().then(() => {
    const deletionOrder = [...createdUsers];
    cy.origin(
      adminOrigin,
      { args: { deletionOrder, base } },
      async ({ deletionOrder, base }) => {
        const credsRaw = localStorage.getItem("credentials");
        let token: string | undefined;
        try {
          token = credsRaw ? JSON.parse(credsRaw).accessToken : undefined;
        } catch {}
        if (!token) {
          throw new Error("No access token in admin origin credentials");
        }
        const headersBase: Record<string, string> = {
          Authorization: `Bearer ${token}`,
          Accept: "application/json, text/plain, */*",
        };
        for (const u of deletionOrder) {
          if (!u.userId) {
            console.log("[e2e] Skip user without id", u.email);
            continue;
          }
          let ok = false;
          try {
            const resp = await fetch(`${base}/user/${u.userId}`, {
              method: "DELETE",
              headers: headersBase,
              credentials: "include",
            });
            if (resp.ok) {
              ok = true;
              console.log(`[e2e] Deleted ${u.email}`);
            } else {
              console.warn(`[e2e] Delete status ${resp.status} for ${u.email}`);
            }
          } catch (e) {
            console.warn(`[e2e] Delete error for ${u.email}:`, e);
          }
        }
      },
    ).then(() => {
      createdUsers.length = 0;
    });
  });
};
