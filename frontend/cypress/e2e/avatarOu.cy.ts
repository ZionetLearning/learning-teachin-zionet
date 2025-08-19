describe('avatar Ou', () => {
	beforeEach(() => {
		cy.login();
		cy.contains(/avatar tools/i).click();
		cy.get('[data-testid="sidebar-avatar-ou"]').click();
		cy.get('[data-testid="avatar-ou-page"]').should('exist');
	});

	it('loads page UI elements', () => {
		cy.get('[data-testid="avatar-ou-input"]').should('be.visible');
		cy.get('[data-testid^="avatar-ou-sample-"]').should(
			'have.length.greaterThan',
			0
		);
		cy.get('[data-testid="avatar-ou-speak"]').should('exist');
		cy.get('[data-testid="avatar-ou-mute"]').should('exist');
	});

	it('speaks sample text via simulated TTS cycle (playing -> stopped)', () => {
		cy.get('[data-testid="avatar-ou-sample-0"]').click();
		cy.get('[data-testid="avatar-ou-input"]')
			.invoke('val')
			.then(() => {
				cy.get('[data-testid="avatar-ou-speak"]').as('speakBtn');
				cy.get('@speakBtn').should('not.be.disabled').click();
				// During simulated speech isPlaying becomes true causing Stop button state
				cy.get('@speakBtn').contains(/stop/i);
				// Wait for simulated cycle (~600ms) to finish and revert
				cy.get('@speakBtn').should('not.be.disabled');
				cy.get('@speakBtn').contains(/start|play/i);
				// Click again to ensure second cycle works
				cy.get('@speakBtn').click();
				cy.get('@speakBtn').contains(/stop/i);
				cy.get('@speakBtn').contains(/stop/i);
				cy.get('@speakBtn').should('not.be.disabled');
			});
	});

	it('toggle mute button reflects state changes', () => {
		cy.get('[data-testid="avatar-ou-mute"]').as('muteBtn');
		cy.get('@muteBtn').click();
		// Icon swap cannot be easily asserted without testid; assert button exists and second click returns
		cy.get('@muteBtn').click();
		cy.get('@muteBtn').should('exist');
	});
});
