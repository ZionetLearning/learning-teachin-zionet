# Feature Specification: Migrate i-teach Components to learning-teachin-zionet

**Feature Branch**: `migrate-i-teach-components`  
**Created**: 10/12/2025  
**Input**: Migrate reusable components, pages, hooks, types, and utilities from `i-teach/classroom-flow-demo` to `learning-teachin-zionet` for future use in the i-teach project. Components use Radix UI primitives styled with Tailwind CSS (shadcn/ui style). Refactor to align with learning-teachin-zionet best practices and make styling easily changeable.

## User Scenarios & Testing _(mandatory)_

### User Story 1 - Developer Can Use Migrated Components in Demo App (Priority: P1)

As a developer working on the i-teach project, I need to see working examples of migrated components and pages in a demo app, so I can understand how to use them and reference the implementation patterns.

**Why this priority**: This is the foundation - without a working demo app, we can't validate the migration or provide reference implementation for the real project.

**Independent Test**: Can be fully tested by running `nx serve classroom-flow-demo` and verifying all pages render correctly with migrated components. Delivers immediate visual feedback and working reference.

**Acceptance Scenarios**:

1. **Given** the demo app is running, **When** I navigate to the landing page, **Then** I see the Index page with mode selection cards
2. **Given** I'm on the InLesson page, **When** I view the page, **Then** I see student cards rendered using migrated StudentCard component
3. **Given** I interact with components, **When** I click buttons or select items, **Then** components respond correctly with proper styling
4. **Given** I view any page, **When** I inspect the code, **Then** I see components use named React imports and extracted styles

---

### User Story 2 - Developer Can Easily Update Component Styling (Priority: P1)

As a developer, I need to update component styling without touching component logic, so when design requirements change, I can quickly adapt components to match new Figma designs.

**Why this priority**: Styling will change when moving to real i-teach project. Making styles easily changeable is critical for maintainability.

**Independent Test**: Can be fully tested by updating styles in `.styles.ts` files and verifying components reflect changes without modifying component logic files. Delivers flexibility for future design updates.

**Acceptance Scenarios**:

1. **Given** a component has styles extracted to `.styles.ts`, **When** I update Tailwind classes in the styles file, **Then** the component appearance changes without modifying component logic
2. **Given** I need to swap the entire theme, **When** I replace the styles file, **Then** all components using those styles update automatically
3. **Given** I extract styles from a component, **When** I view the component file, **Then** I see only component logic, no hard-coded Tailwind classes

---

### User Story 3 - Developer Can Reuse Components in Real i-teach Project (Priority: P2)

As a developer starting the real i-teach project, I need to copy or import migrated components from learning-teachin-zionet, so I don't have to rebuild components from scratch.

**Why this priority**: The main goal is to prepare reusable assets for the real project. Components must be properly structured and documented.

**Independent Test**: Can be fully tested by importing components from `libs/ui/shadcn-components` in a new project and verifying they work correctly. Delivers reusable component library.

**Acceptance Scenarios**:

1. **Given** components are in `libs/ui/shadcn-components`, **When** I import a component in a new app, **Then** it renders correctly with all dependencies resolved
2. **Given** I need a specific component, **When** I check the library exports, **Then** I can find and import it easily
3. **Given** components use shared utilities, **When** I import components, **Then** shared dependencies (utils, hooks) are also available

---

### User Story 4 - Developer Can Reference Page Patterns and Layouts (Priority: P2)

As a developer, I need to see how pages are structured and how components are composed together, so I can replicate similar patterns in the real i-teach project.

**Why this priority**: Page patterns show how components work together. This is valuable reference material even if exact pages aren't reused.

**Independent Test**: Can be fully tested by viewing migrated pages in the demo app and verifying they demonstrate proper component composition patterns. Delivers reference implementation.

**Acceptance Scenarios**:

1. **Given** pages are migrated to demo app, **When** I view page source code, **Then** I see clear patterns for component composition
2. **Given** I need to understand a layout pattern, **When** I view a migrated page, **Then** I can see how components are arranged and styled together
3. **Given** pages use extracted mock data, **When** I need to adapt pages, **Then** I can easily replace mock data with real API calls

---

## Requirements _(mandatory)_

### Functional Requirements

- **FR-001**: System MUST allow importing Radix UI components from `libs/ui/shadcn-components` library
- **FR-002**: System MUST allow importing custom components (DrillInPanel, FAB, StudentCard, GlobalAlert, NavLink) from the library
- **FR-003**: Components MUST use named React imports (`forwardRef`, `type ElementRef`) instead of namespace imports
- **FR-004**: Components MUST have styles extracted to separate `.styles.ts` files for easy updates
- **FR-005**: System MUST allow importing hooks (`use-mobile`, `use-toast`) from `libs/shared` library
- **FR-006**: System MUST allow importing utilities (`cn` function) from `libs/shared` library
- **FR-007**: Demo app MUST render all migrated pages (Index, InLesson, Prep, Review, NotFound) correctly
- **FR-008**: Pages MUST accept props for data instead of using hard-coded mock data
- **FR-009**: Mock data MUST be extracted to `libs/shared/src/mocks/` for reuse
- **FR-010**: System MUST use path aliases configured in `tsconfig.base.json` for imports
- **FR-011**: Components MUST work with Tailwind CSS styling system
- **FR-012**: Design tokens (CSS variables) MUST be synced with Figma design tokens
- **FR-013**: System MUST allow updating component styles by modifying `.styles.ts` files only
- **FR-014**: Components MUST support dark mode and theme switching (using CSS variables and Tailwind dark mode classes)
- **FR-015**: Demo app MUST be publicly accessible without authentication (no auth required for demo purposes)

### Key Entities _(include if feature involves data)_

- **Component**: Reusable UI element built on Radix UI primitives, styled with Tailwind, with styles extracted to `.styles.ts` file
- **Page**: Full page component that composes multiple components, accepts props for data, uses extracted mock data
- **Hook**: Reusable React hook (use-mobile, use-toast) that can be shared across apps
- **Style Configuration**: Tailwind class strings organized in `.styles.ts` files for easy updates
- **Design Token**: HSL CSS variable or Tailwind config value that defines design system (colors, spacing, etc.)
- **Mock Data**: Sample data extracted from pages to `libs/shared/src/mocks/` for testing and demo purposes

## Success Criteria _(mandatory)_

### Measurable Outcomes

- **SC-001**: All 40+ Radix UI components can be imported and used in demo app without errors
- **SC-002**: All 5 custom components render correctly in demo app with proper styling
- **SC-003**: All 5 pages (Index, InLesson, Prep, Review, NotFound) render correctly in demo app
- **SC-004**: 100% of components use named React imports (no `import * as React`)
- **SC-005**: 100% of components with complex styling have styles extracted to `.styles.ts` files
- **SC-006**: Developer can update component styling by modifying only `.styles.ts` files (no component logic changes needed)
- **SC-007**: Demo app builds and runs successfully (`nx serve classroom-flow-demo` works)
- **SC-008**: All components pass TypeScript compilation without errors
- **SC-009**: Design tokens extracted from Figma match migrated CSS variables (within acceptable tolerance)
- **SC-010**: Components can be imported in a new app/project and work correctly (reusability verified)
- **SC-011**: Components support dark mode theme switching (CSS variables and Tailwind dark mode classes work correctly)

### Quality Metrics

- **QM-001**: Code follows learning-teachin-zionet patterns (named imports, path aliases, component structure)
- **QM-002**: All ESLint errors resolved
- **QM-003**: All components have proper TypeScript types
- **QM-004**: Bundle size is optimized (tree-shaking works correctly)
- **QM-005**: Documentation exists for component usage and migration process

## Technical Constraints

- Components must work with existing Nx monorepo structure
- Must use existing path alias configuration
- Must not break existing MUI-based apps (student, teacher, admin)
- Must use React 18+ and TypeScript 5.8+
- Must work with Vite build system
- Must support React Router for routing in demo app

## Out of Scope

- Backend API integration (pages use mock data)
- Authentication/authorization in demo app
- Production deployment configuration
- Performance optimization beyond basic bundle size
- E2E testing setup
- Storybook configuration (nice to have, not required)

## Dependencies

- Nx workspace already exists
- React and TypeScript already configured
- Vite build system already in use
- Tailwind CSS can be added to new library
- Radix UI packages need to be installed
- Figma MCP tools for design token extraction
