describe("Users Page Flow", () => {
  const createdUserIds: string[] = [];
  let usersApiBase: string | undefined;

  beforeEach(() => {
    cy.login();
    cy.contains(/Users/i).click();
    cy.get('[data-testid="users-page"]').should("exist");
    cy.intercept("POST", "**/user").as("createUser");
    cy.intercept("PUT", "**/user/*").as("updateUser");
    cy.intercept("DELETE", "**/user/*").as("deleteUser");
  });

  afterEach(() => {
    if (!createdUserIds.length) return;
    const idsToDelete = [...createdUserIds];
    createdUserIds.length = 0;
    if (!usersApiBase) return;
    idsToDelete.forEach((id) => {
      cy.request({
        method: "DELETE",
        url: `${usersApiBase}/user/${id}`,
        failOnStatusCode: false,
      });
    });
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

    cy.get('[data-testid^="users-item-"]').contains(email).should("exist");
  });

  it("can edit first user inline", () => {
    cy.get("body").then(($b) => {
      const item = $b.find('[data-testid^="users-item-"]').first();
      if (!item.length) {
        cy.log("No users to edit; creating one first");
        const email = `edituser_${Date.now()}@example.com`;
        cy.get('[data-testid="users-create-email"]').clear().type(email);
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

    cy.get('[data-testid^="users-item-"]')
      .first()
      .within(() => {
        cy.get('[data-testid="users-update-btn"]').click();
        const updatedEmail = `updated_${Date.now()}@example.com`;
        cy.wrap(updatedEmail).as("updatedEmail");
        cy.get('[data-testid="users-edit-email"]').clear().type(updatedEmail);
        cy.get('[data-testid="users-edit-password"]')
          .clear()
          .type("NewPass123!");
        cy.get('[data-testid="users-edit-save"]').click();
      });
    cy.wait("@updateUser");
    cy.get("@updatedEmail").then((val) => {
      const expected = String(val);
      cy.get('[data-testid^="users-item-"]')
        .first()
        .find('[data-testid="users-email"]')
        .should("have.text", expected);
    });
  });

  it("can delete a user (first one)", () => {
    cy.get("body").then(($b) => {
      const item = $b.find('[data-testid^="users-item-"]').first();
      if (!item.length) {
        cy.log("No users to delete; creating one first");
        const email = `deluser_${Date.now()}@example.com`;
        cy.get('[data-testid="users-create-email"]').clear().type(email);
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
