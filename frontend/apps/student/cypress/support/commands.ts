/// <reference types="cypress" />

interface TestUser {
  email: string;
  password: string;
  userId?: string;
}

const userExistsInList = (
  $body: JQuery<HTMLElement>,
  email: string,
): boolean => {
  return Array.from($body.find('[data-testid="users-email"]')).some(
    (el) => el.textContent?.trim() === email,
  );
};

let testUser: TestUser | null = null;
const createdEmails = new Set<string>();
const APP_URL = "https://localhost:4000";
const ADMIN_URL = "https://localhost:4002";
const waitForUsersList = () => {
  cy.get('[data-testid="users-table"]', { timeout: 15000 }).should("exist");
};

const searchFor = (q: string) => {
  cy.get('[data-testid="users-search-input"]', { timeout: 10000 })
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

const markCreated = (email: string) => {
  createdEmails.add(email);
  cy.log(`[e2e] Marked as created this run: ${email}`);
};

export const addCreatedEmail = (email: string) => markCreated(email);

const deleteOneByEmailUI = (
  email: string,
  adminCreds: { email: string; password: string },
) => {
  const ensureLoggedIn = () => {
    cy.get("body").then(($b) => {
      const loggedIn =
        $b.find('[data-testid="ps-sidebar-container-test-id"]').length > 0;
      if (!loggedIn) {
        cy.get('[data-testid="auth-email"]', { timeout: 12000 })
          .should("exist")
          .clear()
          .type(adminCreds.email);
        cy.get('[data-testid="auth-password"]')
          .clear()
          .type(adminCreds.password);
        cy.get('[data-testid="auth-submit"]').should("not.be.disabled").click();
        cy.get('[data-testid="ps-sidebar-container-test-id"]', {
          timeout: 15000,
        }).should("exist");
      }
    });
  };

  const goToUsersAndDelete = () => {
    cy.get('[data-testid="sidebar-users"]', { timeout: 20000 }).click();
    cy.get('[data-testid="users-page"]', { timeout: 25000 }).should("exist");
    waitForUsersList();

    locateUserRowByEmail(email).then((rowOrNull) => {
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
        const stillOnUsersPage =
          $b.find('[data-testid="users-page"]').length > 0;

        if (stillOnUsersPage) {
          if (testId) {
            cy.get(`[data-testid="${testId}"]`, { timeout: 10000 }).should(
              "not.exist",
            );
          }
          searchFor(email);
          cy.contains('[data-testid="users-email"]', email).should("not.exist");
          clearSearch();
        } else {
          cy.get('[data-testid="auth-page"]', { timeout: 20000 }).should(
            "exist",
          );
        }
      });
    });
  };

  return cy.location("origin").then((origin) => {
    if (origin === ADMIN_URL) {
      ensureLoggedIn();
      goToUsersAndDelete();
    } else {
      cy.origin(
        ADMIN_URL,
        { args: { email, adminCreds } },
        ({ email, adminCreds }) => {
          const waitForUsersList = () =>
            cy
              .get('[data-testid="users-table"]', { timeout: 15000 })
              .should("exist");
          const searchFor = (q: string) => {
            cy.get('[data-testid="users-search-input"]', { timeout: 10000 })
              .clear()
              .type(q);
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
          const setRowsPerPageAll = () => {
            cy.get('[data-testid="users-pagination"] select', {
              timeout: 10000,
            }).select("-1");
          };
          const locateUserRowByEmail = (email: string) => {
            waitForUsersList();
            searchFor(email);
            return cy.get("body").then(($b) => {
              const exists = Array.from(
                $b.find('[data-testid="users-email"]'),
              ).some((el) => el.textContent?.trim() === email);
              if (exists) {
                return cy
                  .get('[data-testid="users-email"]', { timeout: 10000 })
                  .filter((_, el) => el.textContent?.trim() === email)
                  .first()
                  .closest('tr[data-testid^="users-row-"]');
              }
              clearSearch();
              setRowsPerPageAll();
              waitForUsersList();
              return cy.get("body").then(($b2) => {
                const existsAll = Array.from(
                  $b2.find('[data-testid="users-email"]'),
                ).some((el) => el.textContent?.trim() === email);
                if (!existsAll) return null;
                return cy
                  .get('[data-testid="users-email"]', { timeout: 10000 })
                  .filter((_, el) => el.textContent?.trim() === email)
                  .first()
                  .closest('tr[data-testid^="users-row-"]');
              });
            });
          };

          cy.visit("/");
          cy.get("body").then(($b) => {
            const loggedIn =
              $b.find('[data-testid="ps-sidebar-container-test-id"]').length >
              0;
            if (!loggedIn) {
              cy.get('[data-testid="auth-email"]', { timeout: 12000 })
                .should("exist")
                .clear()
                .type(adminCreds.email);
              cy.get('[data-testid="auth-password"]')
                .clear()
                .type(adminCreds.password);
              cy.get('[data-testid="auth-submit"]')
                .should("not.be.disabled")
                .click();
              cy.get('[data-testid="ps-sidebar-container-test-id"]', {
                timeout: 15000,
              }).should("exist");
            }
          });

          cy.get('[data-testid="sidebar-users"]', { timeout: 15000 }).click();
          cy.get('[data-testid="users-page"]', { timeout: 20000 }).should(
            "exist",
          );
          waitForUsersList();

          locateUserRowByEmail(email).then((rowOrNull) => {
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
            if (testId)
              cy.get(`[data-testid="${testId}"]`, { timeout: 10000 }).should(
                "not.exist",
              );

            searchFor(email);
            cy.contains('[data-testid="users-email"]', email).should(
              "not.exist",
            );
            clearSearch();
          });
        },
      );
    }
  });
};

export const deleteAllCreatedUsers = () => {
  const adminEmail =
    (Cypress.env("ADMIN_EMAIL") as string) ||
    (Cypress.env("TEACHER_EMAIL") as string) ||
    "admin_fixed_user@example.com";
  const adminPassword =
    (Cypress.env("ADMIN_PASSWORD") as string) ||
    (Cypress.env("TEACHER_PASSWORD") as string) ||
    "Passw0rd!";

  const adminCreds = { email: adminEmail, password: adminPassword };

  const deletionSet = new Set<string>(createdEmails);
  const defaultTestEmail =
    (Cypress.env("E2E_TEST_EMAIL") as string) || "e2e_fixed_user@example.com";
  deletionSet.add(defaultTestEmail);
  deletionSet.add(adminEmail);
  const emails = Array.from(deletionSet);
  const nonAdmin = emails.filter((e) => e !== adminEmail);
  const ordered = [...nonAdmin, adminEmail];

  if (ordered.length === 0) {
    cy.log("[e2e] No users to delete.");
    return;
  }

  cy.log(`[e2e] Deleting ${ordered.length} user(s) via admin UI...`);

  cy.wrap(ordered).each((email: string) => {
    deleteOneByEmailUI(email, adminCreds);
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
              if (parsed?.userId) testUser!.userId = parsed.userId;
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

      markCreated(email);

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
              if (parsed?.userId) testUser!.userId = parsed.userId;
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

      markCreated(email);

      cy.wait("@createUser").then((intc) => {
        const body = intc?.response?.body as { userId?: string } | undefined;
        if (body?.userId) testUser!.userId = body.userId;
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
