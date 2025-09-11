import "./commands";
import { deleteCreatedUser } from "./commands";

Cypress.on("uncaught:exception", () => {
  return false;
});

// Run once after all tests in a spec file to remove the deterministic test user.
after(() => {
  deleteCreatedUser();
});
