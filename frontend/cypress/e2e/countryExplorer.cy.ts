describe("Country Explorer", () => {
    beforeEach(() => {
        cy.login();
        cy.contains(/Country Explorer/i).click();
    });

    it("displays title and description", () => {
        cy.contains("Country Explorer").should("exist");
        cy.contains("Search by country name, filter by region and population").should("exist");
    });

    it("shows country cards after load", () => {
        cy.get('[data-testid="country-card"]').should("have.length.greaterThan", 0);
    });

    it("can search by country name", () => {
        cy.get('[data-testid="search-input"]').type("japan");
        cy.get('[data-testid="country-card"]').should("contain.text", "Japan");
    });

    it("can filter by region", () => {
        cy.get('[data-testid="region-select"]').select("Asia");
        cy.get('[data-testid="country-card"]').each(($el) => {
            cy.wrap($el).should("contain.text", "Asia");
        });
    });
    it("filters countries by population less than 10M", () => {
        cy.get('[data-testid="population-select"]').select("< 10M");

        cy.get('[data-testid="country-card"]').each(($el) => {
            cy.wrap($el)
                .contains("strong", "Population") // the <strong> tag
                .parent() // the surrounding <div>
                .invoke("text")
                .then((text) => {
                    console.log("DEBUG - Population text:", text);
                    const match = text.match(/Population:\s*([\d,]+)/);
                    expect(match).to.not.be.null;
                    const number = parseInt(match![1].replace(/,/g, ""));
                    expect(number).to.be.lessThan(10_000_000);
                });
        });
    });

    it("filters countries by population between 10M - 100M", () => {
        cy.get('[data-testid="population-select"]').select("10M – 100M");

        cy.get('[data-testid="country-card"]').each(($el) => {
            cy.wrap($el)
                .contains("strong", "Population") // the <strong> tag
                .parent() // the surrounding <div>
                .invoke("text")
                .then((text) => {
                    console.log("DEBUG - Population text:", text);
                    const match = text.match(/Population:\s*([\d,]+)/);
                    expect(match).to.not.be.null;
                    const number = parseInt(match![1].replace(/,/g, ""));
                    expect(number).to.be.lessThan(100_000_000);
                    expect(number).to.be.greaterThan(10_000_000);
                });
        });
    });

        it("filters countries by population less than 100M", () => {
        cy.get('[data-testid="population-select"]').select("≥ 100M");

        cy.get('[data-testid="country-card"]').each(($el) => {
            cy.wrap($el)
                .contains("strong", "Population") // the <strong> tag
                .parent() // the surrounding <div>
                .invoke("text")
                .then((text) => {
                    console.log("DEBUG - Population text:", text);
                    const match = text.match(/Population:\s*([\d,]+)/);
                    expect(match).to.not.be.null;
                    const number = parseInt(match![1].replace(/,/g, ""));
                    expect(number).to.be.greaterThan(100_000_000);
                });
        });
    });
});
