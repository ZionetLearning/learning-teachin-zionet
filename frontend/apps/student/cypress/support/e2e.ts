import "./commands";
import { deleteAllCreatedUsers } from "./commands";

Cypress.on("uncaught:exception", () => {
  return false;
});

afterEach(() => {
  deleteAllCreatedUsers(true);
});
