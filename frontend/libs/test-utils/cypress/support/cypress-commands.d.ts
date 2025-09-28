declare global {
  namespace Cypress {
    interface Chainable {
      login(): Chainable<void>;
      loginAdmin(): Chainable<void>;
      deleteAllCreatedUsers(): Chainable<void>;
    }
  }
}
export {};
