describe('avatar Da', () => {
	beforeEach(() => {
		cy.login();
		cy.contains(/avatar tools/i).click();
		cy.get('[data-testid="sidebar-avatar-da"]').click();
		cy.get('[data-testid="avatar-da-page"]').should('exist');
	});

	it('renders 3D canvas and remains after resize', () => {
		cy.get('canvas').should('exist');
		cy.viewport(800, 600);
		cy.get('canvas').should('exist');
		cy.viewport(1280, 900);
		cy.get('canvas').should('exist');
	});

	it('accepts typed input and simulates speech cycle (disabled -> enabled)', () => {
		cy.get('[data-testid="avatar-da-input"]', { timeout: 8000 })
			.should('be.visible')
			.as('speechInput')
			.type('שלום');
		cy.get('@speechInput').should('have.value', 'שלום');
		cy.get('[data-testid="avatar-da-speak"]').as('speakBtn');
		cy.get('@speakBtn').should('not.be.disabled').click();
		cy.get('@speakBtn').should('be.disabled');
		cy.get('@speechInput').should('be.disabled');
		cy.get('@speakBtn').should('not.be.disabled');
		cy.get('@speechInput').should('not.be.disabled');
	});
});
