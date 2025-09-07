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

Cypress.Commands.add("login", () => {
  const appUrl = "https://localhost:4000";
  const email = `e2e_user_${Date.now()}_${Math.floor(Math.random() * 1e6)}@example.com`;
  const password = "Passw0rd!";
  const record: { email: string; password: string; userId?: string } = {
    email,
    password,
  };
  createdUsers.push(record);
  Cypress.env("e2eUser", record);
  computeUsersApiBase();
  cy.log(`[e2e] Creating test user via signup: ${email}`);
  cy.intercept("POST", "**/user").as("createUser");
  cy.visit(appUrl + "/");
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
      record.userId = body.userId;
      cy.log(`[e2e] Captured created userId: ${body.userId}`);
    } else {
      cy.log("[e2e] Warning: userId not present in createUser response body");
    }
  });
});

export const deleteAllCreatedUsers = (verify = true) => {
  if (!createdUsers.length) return;

  const adminOrigin = "https://localhost:4002";
  // Use first created user's credentials to sign into admin; delete this user last.
  const loginUser = createdUsers[0];

  const base =
    usersApiBase || Cypress.env("usersApiBase") || computeUsersApiBase();
  cy.log(`[e2e] Deleting ${createdUsers.length} test user(s)`);

  // Login to admin origin (once) using cy.origin for cross-origin context.
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
    const deletionOrder =
      createdUsers.length > 1
        ? [...createdUsers.slice(1), loginUser]
        : [...createdUsers];
    // Perform deletions inside admin origin so its localStorage token is used.
    cy.origin(
      adminOrigin,
      { args: { deletionOrder, base, verify } },
      async ({ deletionOrder, base, verify }) => {
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

        const sleep = (ms: number) => new Promise((r) => setTimeout(r, ms));

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
          if (verify && ok) {
            const attempts = 3;
            for (let attempt = 1; attempt <= attempts; attempt++) {
              try {
                const listResp = await fetch(`${base}/user-list`, {
                  headers: headersBase,
                  credentials: "include",
                });
                if (!listResp.ok) break;
                let data: any = null;
                try {
                  data = await listResp.json();
                } catch {}
                if (Array.isArray(data)) {
                  const found = data.some(
                    (r: any) => r?.userId === u.userId || r?.email === u.email,
                  );
                  if (!found) {
                    console.log(`[e2e] Verified deletion of ${u.email}`);
                    break;
                  }
                  if (attempt === attempts) {
                    console.warn(
                      `[e2e] WARNING: User ${u.email} still present after deletion attempts`,
                    );
                  } else {
                    await sleep(300);
                  }
                } else {
                  console.warn(
                    "[e2e] Unexpected user-list response shape; skipping verification",
                  );
                  break;
                }
              } catch (e) {
                if (attempt === attempts) {
                  console.warn(
                    `[e2e] Verification failed for ${u.email}: ${(e as any)?.message}`,
                  );
                } else {
                  await sleep(300);
                }
              }
            }
          }
        }
      },
    );
  });
};
