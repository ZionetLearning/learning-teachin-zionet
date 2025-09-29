import "@test-utils/cypress";
import { deleteAllCreatedUsers } from "@test-utils/cypress";

Cypress.on("uncaught:exception", () => false);

after(() => {
  deleteAllCreatedUsers();
});
