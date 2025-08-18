describe('e2e spec', () => {
	it('login and logout test', function () {
		cy.visit('http://localhost:5173/');
		cy.get('.active').should('have.class', 'active');
		cy.get(':nth-child(1) > .authPageInput-0-2-9').clear();
		cy.get(':nth-child(1) > .authPageInput-0-2-9').type('admin@admin.com');
		cy.get(':nth-child(2) > .authPageInput-0-2-9').clear();
		cy.get(':nth-child(2) > .authPageInput-0-2-9').type('admin123');
		cy.get('.authPageSubmit-0-2-11').should('be.enabled');
		cy.get('.authPageSubmit-0-2-11').click();
		cy.get(
			'[data-testid="ps-sidebar-container-test-id"] > :nth-child(2) > .css-ewdv3l > .ps-menuitem-root > [data-testid="ps-menu-button-test-id"] > .ps-menu-label'
		).should('be.visible');
		cy.get(
			'[data-testid="ps-sidebar-container-test-id"] > :nth-child(2) > .css-ewdv3l > .ps-menuitem-root > [data-testid="ps-menu-button-test-id"] > .ps-menu-label'
		).click();
		cy.get('.authPageBackground-0-2-1').click();
		cy.get('.authPageTitle-0-2-3').should('be.visible');
	});

	it('switch translation test', function () {
		cy.visit('http://localhost:5173/');
		cy.get(':nth-child(1) > .authPageInput-0-2-9').clear();
		cy.get(':nth-child(1) > .authPageInput-0-2-9').type('admin@admin.com');
		cy.get(':nth-child(2) > .authPageInput-0-2-9').clear();
		cy.get(':nth-child(2) > .authPageInput-0-2-9').type('admin123');
		cy.get('.authPageSubmit-0-2-11').click();
		cy.get(
			':nth-child(1) > :nth-child(1) > :nth-child(2) > :nth-child(1) > .ps-menu-label'
		).should('be.visible');
		cy.get(
			':nth-child(1) > :nth-child(1) > :nth-child(2) > :nth-child(1) > .ps-menu-label'
		).click();
		cy.get(
			'.css-17z6eir > [data-testid="ps-menu-button-test-id"] > .ps-menu-label'
		).should('have.text', 'EN');
		cy.get(
			'.ps-menuitem-root.ps-open > [data-testid="ps-submenu-content-test-id"] > .css-ewdv3l > .css-1ncp32h > [data-testid="ps-menu-button-test-id"] > .ps-menu-label'
		).should('have.text', 'HE');
		cy.get(
			'.ps-menuitem-root.ps-open > [data-testid="ps-submenu-content-test-id"] > .css-ewdv3l > .css-1ncp32h > [data-testid="ps-menu-button-test-id"] > .ps-menu-label'
		).click();
		cy.get(
			'.css-17z6eir > [data-testid="ps-menu-button-test-id"] > .ps-menu-label'
		).should('have.text', 'עברית');
		cy.get(
			'.ps-menuitem-root.ps-open > [data-testid="ps-submenu-content-test-id"] > .css-ewdv3l > .css-1ncp32h > [data-testid="ps-menu-button-test-id"] > .ps-menu-label'
		).should('have.text', 'אנגלית');
		cy.get('.MuiTypography-h4').click();
		cy.get('.MuiTypography-h4').should('be.visible');
		cy.get(
			'.ps-menuitem-root.ps-open > [data-testid="ps-submenu-content-test-id"] > .css-ewdv3l > .css-1ncp32h > [data-testid="ps-menu-button-test-id"] > .ps-menu-label'
		).click();
		cy.get('.MuiTypography-h4').click();
		cy.get('.MuiTypography-h4').should('be.visible');
	});
});
