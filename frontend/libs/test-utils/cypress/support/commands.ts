/// <reference types="cypress" />
/// <reference path="./cypress-commands.d.ts" />

import { addCreatedEmail, createdEmails } from "./helpers";

const APP_URL = "https://localhost:4000";
const ADMIN_URL = "https://localhost:4002";

interface TestUser {
  email: string;
  password: string;
  userId?: string;
}

let testUser: TestUser | null = null;

const waitForUsersList = () => {
  cy.get('[data-testid="users-table"]', { timeout: 20000 }).should("exist");
};

const searchFor = (q: string) => {
  cy.get('[data-testid="users-search-input"]', { timeout: 15000 })
    .should("be.visible")
    .clear()
    .type(q, { delay: 0 });
};

const clearSearch = () => {
  cy.get("body").then(($b) => {
    if ($b.find('[data-testid="users-search-clear"]').length) {
      cy.get('[data-testid="users-search-clear"]').click();
    } else {
      cy.get('[data-testid="users-search-input"]').clear();
    }
  });
};

export const locateUserRowByEmail = (email: string) => {
  waitForUsersList();
  searchFor(email);
  return cy.get("body").then(($b) => {
    const $match = $b
      .find('[data-testid="users-email"]')
      .filter((_, el) => el.textContent?.trim() === email);
    if ($match.length) {
      const $row = $match.first().closest('tr[data-testid^="users-row-"]');
      return cy.wrap($row as JQuery<HTMLElement>);
    }

    clearSearch();
    waitForUsersList();
    return cy.get("body").then(($b2) => {
      const $match2 = $b2
        .find('[data-testid="users-email"]')
        .filter((_, el) => el.textContent?.trim() === email);
      if (!$match2.length) return cy.wrap(null);
      const $row2 = $match2.first().closest('tr[data-testid^="users-row-"]');
      return cy.wrap($row2 as JQuery<HTMLElement>);
    });
  });
};

type AdminCreds = { email: string; password: string };

const buildDeletionSet = (): {
  ordered: string[];
  adminCreds: AdminCreds;
  adminEmail: string;
} => {
  const adminEmail =
    (Cypress.env("ADMIN_EMAIL") as string) ||
    (Cypress.env("TEACHER_EMAIL") as string) ||
    "admin_fixed_user@example.com";
  const adminPassword =
    (Cypress.env("ADMIN_PASSWORD") as string) ||
    (Cypress.env("TEACHER_PASSWORD") as string) ||
    "Passw0rd!";
  const adminCreds: AdminCreds = { email: adminEmail, password: adminPassword };

  const deletionSet = new Set<string>(createdEmails);

  const testEnvEmail = (Cypress.env("E2E_TEST_EMAIL") as string) || null;
  const defaultTestEmail = "e2e_fixed_user@example.com";
  const legacyTestEmail = "e2e_users_fixed_user@example.com";
  [testEnvEmail, defaultTestEmail, legacyTestEmail].forEach((e) => {
    if (e) deletionSet.add(e);
  });

  const adminEnvEmail = (Cypress.env("ADMIN_EMAIL") as string) || null;
  const teacherEnvEmail = (Cypress.env("TEACHER_EMAIL") as string) || null;
  const defaultAdminEmail = "admin_fixed_user@example.com";
  [adminEnvEmail, teacherEnvEmail, defaultAdminEmail, adminEmail].forEach(
    (e) => {
      if (e) deletionSet.add(e);
    },
  );

  const emails = Array.from(deletionSet);
  const nonAdmin = emails.filter((e) => e !== adminEmail);
  const ordered = [...nonAdmin, adminEmail];

  return { ordered, adminCreds, adminEmail };
};

const ensureAdminLoggedInSame = (adminCreds: AdminCreds) => {
  cy.get("body").then(($b) => {
    const loggedIn =
      $b.find('[data-testid="ps-sidebar-container-test-id"]').length > 0;
    if (loggedIn) return;
    cy.visit(ADMIN_URL + "/");
    if (
      $b.find('[data-testid="auth-email"]').length === 0 &&
      $b.find('[data-testid="auth-tab-login"]').length > 0
    ) {
      cy.get('[data-testid="auth-tab-login"]').click();
    }
    cy.get('[data-testid="auth-email"]', { timeout: 15000 })
      .should("exist")
      .clear()
      .type(adminCreds.email);
    cy.get('[data-testid="auth-password"]').clear().type(adminCreds.password);
    cy.get('[data-testid="auth-submit"]').should("not.be.disabled").click();
    const waitForSidebarOrSignup = (attempt = 0) => {
      cy.get("body").then(($b2) => {
        const hasSidebar =
          $b2.find('[data-testid="ps-sidebar-container-test-id"]').length > 0;
        if (hasSidebar) return;
        if (attempt >= 3) {
          if ($b2.find('[data-testid="auth-tab-signup"]').length) {
            cy.get('[data-testid="auth-tab-signup"]').click();
          }
          cy.get('[data-testid="auth-email"]').clear().type(adminCreds.email);
          cy.get('[data-testid="auth-password"]')
            .clear()
            .type(adminCreds.password);
          cy.get('[data-testid="auth-confirm-password"]')
            .clear()
            .type(adminCreds.password);
          cy.get('input[placeholder*="First" i]').clear().type("Admin");
          cy.get('input[placeholder*="Last" i]').clear().type("User");
          cy.get('[data-testid="auth-submit"]')
            .should("not.be.disabled")
            .click();
          cy.get('[data-testid="ps-sidebar-container-test-id"]', {
            timeout: 20000,
          }).should("exist");
        } else {
          cy.wait(1000);
          waitForSidebarOrSignup(attempt + 1);
        }
      });
    };
    waitForSidebarOrSignup(0);
  });
};

const navigateToUsersPageSame = () => {
  cy.get('[data-testid="sidebar-users"]', { timeout: 20000 }).click();
  cy.get('[data-testid="users-page"]', { timeout: 25000 }).should("exist");
  waitForUsersList();
};

const deleteUserFromUISame = (email: string) => {
  return locateUserRowByEmail(email).then((rowOrNull) => {
    if (!rowOrNull) {
      cy.log(`[e2e] User '${email}' not present; nothing to delete.`);
      return;
    }
    const $row = rowOrNull as unknown as JQuery<HTMLElement>;
    const testId = $row.attr("data-testid");
    cy.on("window:confirm", () => true);
    cy.wrap($row)
      .find('[data-testid="users-delete-btn"]')
      .click({ force: true });
    cy.get("body", { timeout: 15000 }).then(($b) => {
      const stillOnUsersPage = $b.find('[data-testid="users-page"]').length > 0;
      if (stillOnUsersPage) {
        if (testId) {
          cy.get(`[data-testid=\"${testId}\"]`, { timeout: 10000 }).should(
            "not.exist",
          );
        }
        searchFor(email);
        cy.contains('[data-testid=\"users-email\"]', email).should("not.exist");
        clearSearch();
      } else {
        cy.get('[data-testid="auth-page"]', { timeout: 20000 }).should("exist");
      }
    });
  });
};

const forceLogoutSameOrigin = () => {
  cy.get("body").then(($b) => {
    const onAuth =
      $b.find('[data-testid="auth-page"]').length > 0 ||
      $b.find('[data-testid="auth-email"]').length > 0;
    if (onAuth) return;
    cy.log("[e2e] Forcing logout after cleanup (same-origin)");
    cy.window().then((win) => {
      try {
        win.localStorage.clear();
      } catch {}
    });
    cy.visit(ADMIN_URL + "/");
    cy.get('[data-testid="auth-page"]', { timeout: 20000 }).should("exist");
  });
};

const handleSameOriginDeletion = (
  ordered: string[],
  adminCreds: AdminCreds,
) => {
  ensureAdminLoggedInSame(adminCreds);
  navigateToUsersPageSame();
  cy.wrap(ordered)
    .each((email: string) => deleteUserFromUISame(email))
    .then(() => forceLogoutSameOrigin());
};

const handleCrossOriginDeletion = (
  ordered: string[],
  adminCreds: AdminCreds,
) => {
  cy.origin(
    ADMIN_URL,
    { args: { emails: ordered, adminCreds } },
    ({ emails, adminCreds }: { emails: string[]; adminCreds: AdminCreds }) => {
      cy.visit("/");
      const ensureAdminLoggedIn = () => {
        cy.get("body").then(($b) => {
          const loggedIn =
            $b.find('[data-testid=\"ps-sidebar-container-test-id\"]').length >
            0;
          if (loggedIn) return;
          if (
            $b.find('[data-testid=\"auth-email\"]').length === 0 &&
            $b.find('[data-testid=\"auth-tab-login\"]').length > 0
          ) {
            cy.get('[data-testid=\"auth-tab-login\"]').click();
          }
          cy.get('[data-testid=\"auth-email\"]', { timeout: 15000 })
            .should("exist")
            .clear()
            .type(adminCreds.email);
          cy.get('[data-testid=\"auth-password\"]')
            .clear()
            .type(adminCreds.password);
          cy.get('[data-testid=\"auth-submit\"]')
            .should("not.be.disabled")
            .click();
          const waitForSidebarOrSignup = (attempt = 0) => {
            cy.get("body").then(($b2) => {
              const hasSidebar =
                $b2.find('[data-testid=\"ps-sidebar-container-test-id\"]')
                  .length > 0;
              if (hasSidebar) return;
              if (attempt >= 3) {
                if ($b2.find('[data-testid=\"auth-tab-signup\"]').length) {
                  cy.get('[data-testid=\"auth-tab-signup\"]').click();
                }
                cy.get('[data-testid=\"auth-email\"]')
                  .clear()
                  .type(adminCreds.email);
                cy.get('[data-testid=\"auth-password\"]')
                  .clear()
                  .type(adminCreds.password);
                cy.get('[data-testid=\"auth-confirm-password\"]')
                  .clear()
                  .type(adminCreds.password);
                cy.get('input[placeholder*=\"First\" i]').clear().type("Admin");
                cy.get('input[placeholder*=\"Last\" i]').clear().type("User");
                cy.get('[data-testid=\"auth-submit\"]')
                  .should("not.be.disabled")
                  .click();
                cy.get('[data-testid=\"ps-sidebar-container-test-id\"]', {
                  timeout: 20000,
                }).should("exist");
              } else {
                cy.wait(1000);
                waitForSidebarOrSignup(attempt + 1);
              }
            });
          };
          waitForSidebarOrSignup(0);
        });
      };

      const waitForUsersListInner = () => {
        cy.get('[data-testid=\"users-table\"]', { timeout: 20000 }).should(
          "exist",
        );
      };
      const searchForInner = (q: string) => {
        cy.get('[data-testid=\"users-search-input\"]', { timeout: 15000 })
          .clear()
          .type(q);
      };
      const clearSearchInner = () => {
        cy.get("body").then(($b) => {
          if ($b.find('[data-testid=\"users-search-clear\"]').length) {
            cy.get('[data-testid=\"users-search-clear\"]').click();
          } else {
            cy.get('[data-testid=\"users-search-input\"]').clear();
          }
        });
      };
      const locateUserRowByEmailInner = (email: string) => {
        waitForUsersListInner();
        searchForInner(email);
        return cy.get("body").then(($b) => {
          const exists = Array.from(
            $b.find('[data-testid=\"users-email\"]'),
          ).some((el) => el.textContent?.trim() === email);
          if (exists) {
            return cy
              .get('[data-testid=\"users-email\"]', { timeout: 15000 })
              .filter((_, el) => el.textContent?.trim() === email)
              .first()
              .closest('tr[data-testid^=\"users-row-\"]');
          }
          clearSearchInner();
          waitForUsersListInner();
          return cy.get("body").then(($b2) => {
            const existsAll = Array.from(
              $b2.find('[data-testid=\"users-email\"]'),
            ).some((el) => el.textContent?.trim() === email);
            if (!existsAll) return null;
            return cy
              .get('[data-testid=\"users-email\"]', { timeout: 15000 })
              .filter((_, el) => el.textContent?.trim() === email)
              .first()
              .closest('tr[data-testid^=\"users-row-\"]');
          });
        });
      };

      const deleteUserFromUICross = (email: string) => {
        return locateUserRowByEmailInner(email).then((rowOrNull) => {
          if (!rowOrNull) {
            cy.log(`[e2e] User '${email}' not present; nothing to delete.`);
            return;
          }
          const $row = rowOrNull as unknown as JQuery<HTMLElement>;
          const testId = $row.attr("data-testid");
          cy.on("window:confirm", () => true);
          cy.wrap($row)
            .find('[data-testid=\"users-delete-btn\"]')
            .click({ force: true });
          cy.get("body", { timeout: 15000 }).then(($b) => {
            const stillOnUsersPage =
              $b.find('[data-testid=\"users-page\"]').length > 0;
            if (stillOnUsersPage) {
              if (testId) {
                cy.get(`[data-testid=\"${testId}\"]`, {
                  timeout: 10000,
                }).should("not.exist");
              }
              searchForInner(email);
              cy.contains('[data-testid=\"users-email\"]', email).should(
                "not.exist",
              );
              clearSearchInner();
            } else {
              cy.get('[data-testid=\"auth-page\"]', { timeout: 20000 }).should(
                "exist",
              );
            }
          });
        });
      };

      const forceLogoutCrossOrigin = () => {
        cy.get("body").then(($b) => {
          const onAuth =
            $b.find('[data-testid=\"auth-page\"]')?.length > 0 ||
            $b.find('[data-testid=\"auth-email\"]').length > 0;
          if (onAuth) return;
          cy.log("[e2e] Forcing logout after cleanup (cross-origin)");
          cy.window().then((win) => {
            try {
              win.localStorage.clear();
            } catch {}
          });
          cy.visit("/");
          cy.get('[data-testid=\"auth-page\"]', { timeout: 20000 }).should(
            "exist",
          );
        });
      };

      ensureAdminLoggedIn();
      cy.get('[data-testid=\"sidebar-users\"]', { timeout: 20000 }).click();
      cy.get('[data-testid=\"users-page\"]', { timeout: 25000 }).should(
        "exist",
      );
      cy.wrap(emails)
        .each((email: string) => deleteUserFromUICross(email))
        .then(() => forceLogoutCrossOrigin());
    },
  );
};

export const deleteAllCreatedUsers = () => {
  const { ordered, adminCreds } = buildDeletionSet();

  if (ordered.length === 0) {
    cy.log("[e2e] No users to delete.");
    return;
  }

  cy.log(`[e2e] Deleting ${ordered.length} user(s) via admin UI...`);

  cy.location("origin").then((origin) => {
    if (origin === ADMIN_URL) {
      handleSameOriginDeletion(ordered, adminCreds);
    } else {
      handleCrossOriginDeletion(ordered, adminCreds);
    }
  });

  cy.then(() => {
    createdEmails.clear();
    testUser = null;
    cy.log("[e2e] Cleared createdEmails + testUser after cleanup.");
  });
};

Cypress.Commands.add("login", () => {
  const email =
    (Cypress.env("E2E_TEST_EMAIL") as string) || "e2e_fixed_user@example.com";
  const password = (Cypress.env("E2E_TEST_PASSWORD") as string) || "Passw0rd!";

  if (!testUser || testUser.email !== email) testUser = { email, password };
  Cypress.env("e2eUser", testUser);

  cy.log(`[e2e] Login (or signup) with deterministic test user ${email}`);
  cy.intercept("POST", "**/auth/login").as("loginRequest");
  cy.intercept("POST", "**/user").as("createUser");

  cy.visit(APP_URL + "/");
  cy.get('[data-testid=\"auth-tab-login\"]').click();
  cy.get('[data-testid=\"auth-email\"]').clear().type(email);
  cy.get('[data-testid=\"auth-password\"]').clear().type(password);
  cy.get('[data-testid=\"auth-submit\"]').should("not.be.disabled").click();

  cy.wait("@loginRequest").then((loginInt) => {
    const status = loginInt?.response?.statusCode;
    if (status === 200) {
      cy.log(`[e2e] Logged in existing deterministic user ${email}`);
      cy.get('[data-testid=\"ps-sidebar-container-test-id\"]').should("exist");
      if (!testUser!.userId) {
        cy.window().then((win) => {
          try {
            const credsRaw = win.localStorage.getItem("credentials");
            if (credsRaw) {
              const parsed = JSON.parse(credsRaw);
              if (parsed?.userId) testUser!.userId = parsed.userId;
            }
          } catch {}
        });
      }
    } else {
      cy.log(`
        [e2e] Login failed with status ${status}; attempting signup for ${email}`);
      cy.get('[data-testid=\"auth-tab-signup\"]').click();
      cy.get('[data-testid=\"auth-email\"]').clear().type(email);
      cy.get('[data-testid=\"auth-password\"]').clear().type(password);
      cy.get('[data-testid=\"auth-confirm-password\"]').clear().type(password);
      cy.get('input[placeholder*=\"First\" i]').clear().type("E2E");
      cy.get('input[placeholder*=\"Last\" i]').clear().type("User");
      cy.get('[data-testid=\"auth-submit\"]').should("not.be.disabled").click();
      cy.get('[data-testid=\"ps-sidebar-container-test-id\"]').should("exist");

      addCreatedEmail(email);

      cy.wait("@createUser").then((intc) => {
        const body = intc?.response?.body as { userId?: string } | undefined;
        if (body?.userId) testUser!.userId = body.userId;
      });
    }
  });
});

Cypress.Commands.add("loginAdmin", () => {
  const email =
    (Cypress.env("ADMIN_EMAIL") as string) ||
    (Cypress.env("TEACHER_EMAIL") as string) ||
    "admin_fixed_user@example.com";
  const password =
    (Cypress.env("ADMIN_PASSWORD") as string) ||
    (Cypress.env("TEACHER_PASSWORD") as string) ||
    "Passw0rd!";

  if (!testUser || testUser.email !== email) testUser = { email, password };
  Cypress.env("e2eAdminUser", testUser);

  addCreatedEmail(email);

  cy.log(`[e2e] Admin login (or signup) deterministic user ${email}`);
  cy.intercept("POST", "**/auth/login").as("loginRequest");
  cy.intercept("POST", "**/user").as("createUser");

  cy.visit(ADMIN_URL + "/");
  cy.get('[data-testid=\"auth-tab-login\"]').click();
  cy.get('[data-testid=\"auth-email\"]').clear().type(email);
  cy.get('[data-testid=\"auth-password\"]').clear().type(password);
  cy.get('[data-testid=\"auth-submit\"]').should("not.be.disabled").click();

  cy.wait("@loginRequest").then((loginInt) => {
    const status = loginInt?.response?.statusCode;
    if (status === 200) {
      cy.log(`[e2e] Admin logged in existing deterministic user ${email}`);
      cy.get('[data-testid=\"ps-sidebar-container-test-id\"]').should("exist");
      if (!testUser!.userId) {
        cy.window().then((win) => {
          try {
            const credsRaw = win.localStorage.getItem("credentials");
            if (credsRaw) {
              const parsed = JSON.parse(credsRaw);
              if (parsed?.userId) testUser!.userId = parsed.userId;
            }
          } catch {}
        });
      }
    } else {
      cy.log(`[e2e] Admin login failed ${status}; attempting signup ${email}`);
      cy.get('[data-testid=\"auth-tab-signup\"]').click();
      cy.get('[data-testid=\"auth-email\"]').clear().type(email);
      cy.get('[data-testid=\"auth-password\"]').clear().type(password);
      cy.get('[data-testid=\"auth-confirm-password\"]').clear().type(password);
      cy.get('input[placeholder*=\"First\" i]').clear().type("Admin");
      cy.get('input[placeholder*=\"Last\" i]').clear().type("User");
      cy.get('[data-testid=\"auth-submit\"]').should("not.be.disabled").click();
      cy.get('[data-testid=\"ps-sidebar-container-test-id\"]').should("exist");

      addCreatedEmail(email);

      cy.wait("@createUser").then((intc) => {
        const body = intc?.response?.body as { userId?: string } | undefined;
        if (body?.userId) testUser!.userId = body.userId;
      });
    }
  });
});

export {};
