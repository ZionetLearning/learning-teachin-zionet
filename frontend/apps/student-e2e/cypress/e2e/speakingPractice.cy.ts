describe("speaking practice", () => {
  const startGame = () => {
    cy.login();
    cy.contains(/practice tools/i).click();
    cy.get('[data-testid="sidebar-speaking"]')
      .scrollIntoView()
      .click({ timeout: 10000 });
    // Wait until SignalR reports connected (set on window by provider)
    cy.window({ timeout: 15000 }).should((w) => {
      const status = (w as unknown as { __signalRStatus?: string })
        .__signalRStatus;
      expect(status, "SignalR status should become connected").to.equal(
        "connected",
      );
    });

    // Then start the game (button should now be safe to click)
    cy.get('[data-testid="game-config-start"]', { timeout: 10000 })
      .should("not.be.disabled")
      .click();

    waitForSentences();
  };

  const waitForSentences = () => {
    cy.get('[data-testid="speaking-practice-page"]', { timeout: 20000 }).should(
      "exist",
    );
    cy.get('[data-testid="speaking-index"]', { timeout: 20000 }).should(
      ($idx) => {
        expect($idx.text().trim()).to.match(/^[0-9]+ \/ [0-9]+$/);
      },
    );
    cy.get('[data-testid="speaking-phrase"]', { timeout: 20000 }).should(
      ($p) => {
        expect($p.text().trim()).to.not.equal("");
      },
    );
  };

  beforeEach(() => {
    startGame();
  });

  it("navigates phrases forward and backward (index assertion)", () => {
    cy.get('[data-testid="speaking-index"]')
      .invoke("text")
      .then((start) => {
        const [startCurrent, total] = start.split("/").map((s) => s.trim());
        const startNum = parseInt(startCurrent, 10);
        if (total === startCurrent) {
          cy.log("Single sentence set; skipping navigation assertions.");
          return;
        }
        cy.get('[data-testid="speaking-next"]')
          .should("not.be.disabled")
          .click();
        cy.get('[data-testid="speaking-index"]').should(($el) => {
          const [curr] = $el.text().split("/");
          expect(
            parseInt(curr, 10),
            "index after next should increment",
          ).to.be.greaterThan(startNum);
        });
        cy.get('[data-testid="speaking-prev"]')
          .should("not.be.disabled")
          .click();
        cy.get('[data-testid="speaking-index"]').should(($el) => {
          expect($el.text().trim()).to.eq(start.trim());
        });
      });
  });

  it("applies nikud setting changes via config modal (phrase still renders)", () => {
    cy.get('[data-testid="speaking-phrase"]')
      .invoke("text")
      .as("initialPhrase");

    cy.contains(/settings/i).click();
    // Toggle nikud off if currently enabled using stable test id
    cy.get('[data-testid="game-config-nikud"] input[type="checkbox"]').then(
      ($cb) => {
        if ($cb.prop("checked")) {
          cy.wrap($cb).click({ force: true });
        }
      },
    );

    cy.get('[data-testid="game-config-start"]').click();
    waitForSentences();

    cy.get('[data-testid="speaking-phrase"]')
      .should("exist")
      .invoke("text")
      .then((after) => {
        expect(after).to.be.a("string");
        expect(after.length).to.be.greaterThan(0);
      });
  });

  it("resets feedback when navigating to next sentence", () => {
    cy.get('[data-testid="speaking-feedback"]').as("feedback");
    cy.get("@feedback").should(($f) => {
      expect($f.text()).to.be.a("string");
    });

    cy.get('[data-testid="speaking-record"]').then(($btn) => {
      if (!$btn.is(":disabled")) {
        cy.wrap($btn).click();
        cy.wait(250);
        cy.wrap($btn).click();
      } else {
        cy.log("Record button disabled; skipping record simulation.");
      }
    });

    cy.get('[data-testid="speaking-feedback"]').should("exist");
    cy.get('[data-testid="speaking-next"]').then(($next) => {
      if ($next.is(":disabled")) {
        cy.log("Next disabled (single sentence) skipping reset assertion.");
        return;
      }
      cy.wrap($next).click();
      cy.get('[data-testid="speaking-feedback"]').should(($f) => {
        const txt = $f.text().trim();
        const loadingMatch = /loading/i.test(txt);
        expect(txt === "" || loadingMatch).to.eq(true);
      });
    });
  });
});
