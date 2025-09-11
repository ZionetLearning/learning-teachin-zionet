import { locateUserRowByEmail } from "../support/commands";

describe("Users Page Flow (admin app @4002)", () => {
  const TEST_EMAIL = "e2e_users_fixed_user@example.com";
  const UPDATED_EMAIL = "e2e_users_fixed_user+updated@example.com";
  const TEST_PASSWORD = "Passw0rd!";
  const TEST_FIRST = "E2E";
  const TEST_LAST = "Fixed";

  const selectRole = (role: string) => {
    const triggerWithinSelector =
      '[role="button"], [role="combobox"], .MuiSelect-select, [aria-haspopup="listbox"]';
    const triggerSelector = `[data-testid="users-create-role"] ${triggerWithinSelector}`;

    cy.get('[data-testid="users-create-role"]', { timeout: 15000 })
      .should("exist")
      .scrollIntoView()
      .should("be.visible")
      .within(() => {
        cy.get(triggerWithinSelector, { timeout: 10000 })
          .should("be.visible")
          .click({ force: true });
      });

    cy.get("body").then(($body) => {
      if ($body.find('ul[role="listbox"]').length === 0) {
        cy.get(triggerSelector).first().click({ force: true });
        cy.focused().type("{enter}", { force: true });
      }
    });

    cy.get('ul[role="listbox"]', { timeout: 10000 }).should("be.visible");

    cy.get("body").then(($b) => {
      const byValue = $b.find(`ul[role="listbox"] li[data-value="${role}"]`);
      if (byValue.length) {
        cy.wrap(byValue.first()).should("be.visible").click({ force: true });
      } else {
        cy.contains('ul[role="listbox"] li', new RegExp(`^${role}$`, "i"), {
          timeout: 10000,
        })
          .should("be.visible")
          .click({ force: true });
      }
    });

    cy.get('ul[role="listbox"]').should("not.exist");

    cy.get('[data-testid="users-create-role"]').within(() => {
      cy.get(
        '[role="button"], [role="combobox"], .MuiSelect-select, [aria-haspopup="listbox"]',
      ).should("exist");
    });
    cy.get('[data-testid="users-create-role"]').should(($el) => {
      expect($el.text().toLowerCase()).to.include(role.toLowerCase());
    });
  };

  const searchFor = (query: string) => {
    cy.get('[data-testid="users-search-input"]').clear().type(query);
  };

  const clearSearch = () => {
    cy.get("body").then(($b) => {
      const btn = $b.find('[data-testid="users-search-clear"]');
      if (btn.length) cy.get('[data-testid="users-search-clear"]').click();
    });
  };

  const goToUsersPage = () => {
    cy.contains(/Users/i).click();
    cy.get('[data-testid="users-page"]').should("exist");
    cy.get('[data-testid="users-table"]').should("exist");
  };

  const deleteUserIfExistsByEmail = (email: string) => {
    cy.get("body").then(($b) => {
      if ($b.find('[data-testid="users-page"]').length === 0) {
        goToUsersPage();
      }
    });

    searchFor(email);

    cy.get("body").then(($b) => {
      const found = Array.from($b.find('[data-testid="users-email"]')).some(
        (el) => el.textContent?.trim() === email,
      );

      if (found) {
        cy.contains('[data-testid="users-email"]', email, { timeout: 5000 })
          .closest('tr[data-testid^="users-row-"]')
          .as("row");

        cy.window().then((win) => cy.stub(win, "confirm").returns(true));
        cy.get("@row")
          .find('[data-testid="users-delete-btn"]')
          .click({ force: true });

        cy.wait("@deleteUser");
        cy.wait("@getUsers");
        searchFor(email);
        cy.contains('[data-testid="users-email"]', email).should("not.exist");
      }
    });

    clearSearch();
  };

  const createDeterministicUser = (email = TEST_EMAIL) => {
    cy.get('[data-testid="users-create-email"]').clear().type(email);
    cy.get('[data-testid="users-create-first-name"]').clear().type(TEST_FIRST);
    cy.get('[data-testid="users-create-last-name"]').clear().type(TEST_LAST);
    selectRole("student");
    cy.get('[data-testid="users-create-password"]').clear().type(TEST_PASSWORD);
    cy.get('[data-testid="users-create-submit"]').click();

    cy.wait("@createUser");
    cy.wait("@getUsers");

    searchFor(email);
    cy.contains('[data-testid="users-email"]', email, {
      timeout: 10000,
    }).should("exist");
    clearSearch();
  };

  beforeEach(() => {
    cy.loginAdmin();
    goToUsersPage();

    cy.intercept("POST", "**/user").as("createUser");
    cy.intercept("PUT", "**/user/*").as("updateUser");
    cy.intercept("DELETE", "**/user/*").as("deleteUser");
    cy.intercept("GET", "**/user-list").as("getUsers");

    deleteUserIfExistsByEmail(TEST_EMAIL);
    deleteUserIfExistsByEmail(UPDATED_EMAIL);
  });

  afterEach(() => {
    deleteUserIfExistsByEmail(TEST_EMAIL);
    deleteUserIfExistsByEmail(UPDATED_EMAIL);
  });

  it("shows create form and list (may be empty)", () => {
    cy.get('[data-testid="users-create-email"]').should("exist");
    cy.get('[data-testid="users-create-password"]').should("exist");
    cy.get('[data-testid="users-create-submit"]').should("exist");
    cy.get('[data-testid="users-table"]').should("exist");
  });

  it("can create a user with deterministic credentials", () => {
    createDeterministicUser(TEST_EMAIL);

    searchFor(TEST_EMAIL);
    cy.contains('[data-testid="users-email"]', TEST_EMAIL).should("exist");
    clearSearch();
  });

  it("can edit that user inline (change only the email) and reflect in UI + request body", () => {
    const baseEmail = "e2e_users_fixed_user@example.com";
    const updatedEmail = "e2e_users_fixed_user+updated@example.com";

    deleteUserIfExistsByEmail(baseEmail);
    deleteUserIfExistsByEmail(updatedEmail);

    cy.get('[data-testid="users-create-email"]').clear().type(baseEmail);
    cy.get('[data-testid="users-create-first-name"]').clear().type("E2E");
    cy.get('[data-testid="users-create-last-name"]').clear().type("Fixed");
    selectRole("student");
    cy.get('[data-testid="users-create-password"]').clear().type("Passw0rd!");
    cy.intercept("POST", "**/user").as("createUser");
    cy.intercept("GET", "**/user-list").as("getUsers");
    cy.get('[data-testid="users-create-submit"]').click();
    cy.wait("@createUser")
      .its("response.statusCode")
      .should("be.oneOf", [200, 201]);
    cy.wait("@getUsers");

    locateUserRowByEmail(baseEmail).then((rowOrNull) => {
      expect(rowOrNull, "user row should exist after create").to.not.equal(
        null,
      );

      const $row = rowOrNull as unknown as JQuery<HTMLElement>;
      cy.wrap($row).as("editRow");
    });

    cy.get("@editRow").within(() => {
      cy.get('[data-testid="users-update-btn"]').click();
      cy.get('[data-testid="users-edit-email"]').clear().type(updatedEmail);
      cy.intercept("PUT", "**/user/*").as("updateUser");
      cy.get('[data-testid="users-edit-save"]').click();
    });

    cy.wait("@updateUser").then((intc) => {
      expect(intc?.request?.body).to.have.property("email", updatedEmail);
      expect(intc?.request?.body).to.not.have.property("firstName");
      expect(intc?.request?.body).to.not.have.property("lastName");
    });

    cy.wait("@getUsers");
    searchFor(updatedEmail);
    cy.contains('[data-testid="users-email"]', updatedEmail, {
      timeout: 10000,
    }).should("exist");
    clearSearch();
  });

  it("can delete the deterministic user via the UI", () => {
    createDeterministicUser(TEST_EMAIL);

    deleteUserIfExistsByEmail(TEST_EMAIL);

    searchFor(TEST_EMAIL);
    cy.contains('[data-testid="users-email"]', TEST_EMAIL).should("not.exist");
    clearSearch();
  });
});
