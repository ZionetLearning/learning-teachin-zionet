// Cypress custom command type declarations
// Placed in its own d.ts so commands.ts can stay a module without namespace lint issues.

declare namespace Cypress {
  interface Chainable {
    login(email?: string, password?: string): Chainable<void>;
  }
}
