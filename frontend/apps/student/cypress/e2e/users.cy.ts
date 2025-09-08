describe("Users Page Flow (admin app @4002)", () => {
  const createdUserIds: string[] = [];
  let usersApiBase: string | undefined;

  beforeEach(() => {
    cy.loginAdmin();
    cy.contains(/Users/i).click();
    cy.get('[data-testid="users-page"]').should("exist");
    cy.intercept("POST", "**/user").as("createUser");
    cy.intercept("PUT", "**/user/*").as("updateUser");
    cy.intercept("DELETE", "**/user/*").as("deleteUser");
    cy.intercept("GET", "**/user-list").as("getUsers");
  });

  afterEach(() => {
    if (!createdUserIds.length) return;
    const idsToDelete = [...createdUserIds];
    cy.get("body").then(($b) => {
      if ($b.find('[data-testid="users-page"]').length === 0) {
        cy.contains(/Users/i).click();
        cy.get('[data-testid="users-page"]').should("exist");
      }
    });
    idsToDelete.forEach((id) => {
      const rowSelector = `[data-testid="users-item-${id}"]`;
      cy.get("body").then(($b) => {
        if ($b.find(rowSelector).length) {
          cy.window().then((win) => {
            cy.stub(win, "confirm").returns(true);
          });
          cy.get(rowSelector).within(() => {
            cy.get('[data-testid="users-delete-btn"]').click();
          });
          cy.wait("@deleteUser")
            .its("response.statusCode")
            .should("be.oneOf", [200, 204]);
        } else {
          cy.log(`[cleanup] user ${id} not found in UI (already deleted?)`);
        }
      });
    });
    createdUserIds.length = 0;
  });

  it("shows create form and list (may be empty)", () => {
    cy.get('[data-testid="users-create-email"]').should("exist");
    cy.get('[data-testid="users-create-password"]').should("exist");
    cy.get('[data-testid="users-create-submit"]').should("exist");
    cy.get('[data-testid="users-list"]').should("exist");
  });

  interface UserApiPayload {
    userId?: string;
    email?: string;
  }
  it("can create a user (API dependent)", () => {
    const email = `testuser_${Date.now()}@example.com`;
    cy.get('[data-testid="users-create-email"]').clear().type(email);
    cy.get('[data-testid="users-create-first-name"]').clear().type("Test");
    cy.get('[data-testid="users-create-last-name"]').clear().type("User");
    cy.get('[data-testid="users-create-password"]').clear().type("Passw0rd!");
    cy.get('[data-testid="users-create-submit"]').click();

    cy.wait("@createUser").then((interception) => {
      if (interception?.request?.url) {
        usersApiBase = interception.request.url.replace(/\/user$/i, "");
      }
      const body = interception?.response?.body as UserApiPayload | undefined;
      const newId = body?.userId;
      if (newId) createdUserIds.push(newId);
    });
    cy.wait("@getUsers");
    cy.get('[data-testid^="users-item-"]').contains(email).should("exist");
  });

  it("can edit a freshly created user inline (isolated)", () => {
    const originalEmail = `edituser_${Date.now()}@example.com`;
    cy.get('[data-testid="users-create-email"]').clear().type(originalEmail);
    cy.get('[data-testid="users-create-first-name"]').clear().type("Edit");
    cy.get('[data-testid="users-create-last-name"]').clear().type("User");
    cy.get('[data-testid="users-create-password"]').clear().type("Passw0rd!");
    cy.get('[data-testid="users-create-submit"]').click();
    cy.wait("@createUser").then((interception) => {
      if (interception?.request?.url) {
        usersApiBase = interception.request.url.replace(/\/user$/i, "");
      }
      const body = interception?.response?.body as UserApiPayload | undefined;
      const newId = body?.userId;
      if (newId) {
        createdUserIds.push(newId);
        cy.wrap(newId).as("editUserId");
      }
    });
    cy.wait("@getUsers");
    const updatedEmail = `updated_${Date.now()}@example.com`;
    cy.wrap(updatedEmail).as("updatedEmail");
    cy.get("@editUserId").then((id) => {
      cy.get(`[data-testid="users-item-${id}"]`).within(() => {
        cy.get('[data-testid="users-update-btn"]').click();
        cy.get('[data-testid="users-edit-email"]').clear().type(updatedEmail);
        cy.get('[data-testid="users-edit-save"]').click();
      });
    });
    cy.wait("@updateUser").then((interception) => {
      expect(interception?.request?.body).to.have.property(
        "email",
        updatedEmail,
      );
      expect(interception?.request?.body).to.not.have.property("firstName");
      expect(interception?.request?.body).to.not.have.property("lastName");
    });
    cy.wait("@getUsers");
    cy.get("@editUserId").then((id) => {
      cy.get("@updatedEmail").then((val) => {
        const expected = String(val);
        cy.get(
          `[data-testid="users-item-${id}"] [data-testid="users-email"]`,
        ).should("have.text", expected);
      });
    });
  });

  it("can delete a user (first one)", () => {
    cy.get("body").then(($b) => {
      const item = $b.find('[data-testid^="users-item-"]').first();
      if (!item.length) {
        cy.log("No users to delete; creating one first");
        const email = `deluser_${Date.now()}@example.com`;
        cy.get('[data-testid="users-create-email"]').clear().type(email);
        cy.get('[data-testid="users-create-first-name"]').clear().type("Del");
        cy.get('[data-testid="users-create-last-name"]').clear().type("User");
        cy.get('[data-testid="users-create-password"]')
          .clear()
          .type("Passw0rd!");
        cy.get('[data-testid="users-create-submit"]').click();
        cy.wait("@createUser").then((interception) => {
          if (interception?.request?.url) {
            usersApiBase = interception.request.url.replace(/\/user$/i, "");
          }
          const body = interception?.response?.body as
            | UserApiPayload
            | undefined;
          const newId = body?.userId;
          if (newId) createdUserIds.push(newId);
        });
      }
    });

    cy.get('[data-testid^="users-item-"]').first().as("firstItem");
    cy.get("@firstItem")
      .find('[data-testid="users-email"]')
      .invoke("text")
      .then((textBefore) => {
        cy.get("@firstItem")
          .invoke("attr", "data-testid")
          .then((attr) => {
            const id = attr?.replace("users-item-", "");
            cy.window().then((win) => {
              cy.stub(win, "confirm").returns(true);
            });
            cy.get("@firstItem")
              .find('[data-testid="users-delete-btn"]')
              .click();
            cy.wait("@deleteUser");
            if (id) {
              const idx = createdUserIds.indexOf(id);
              if (idx >= 0) createdUserIds.splice(idx, 1);
            }
          });
        cy.contains('[data-testid="users-email"]', textBefore.trim()).should(
          "not.exist",
        );
      });
  });
});
