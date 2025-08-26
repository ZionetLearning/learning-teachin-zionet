# Frontend Workspace

Multi-app [Nx workspace](https://nx.dev) with **Student**, **Teacher**, and **Admin** apps  

Run `npx nx graph` to visually explore the project structure.  

---

## Apps in this Workspace  

- **Student App** → `apps/student`  
- **Teacher App** → `apps/teacher`  
- **Admin App** → `apps/admin`  

Shared libraries live under `libs/`.

---

## Scripts  

 
```sh
### Lint 
npm run lint              # Lint all projects
npm run lint:affected     # Lint only affected projects
npm run lint:fix          # Lint all and auto-fix issues
npm run lint:affected:fix # Lint only affected and auto-fix
npm run lint:student
npm run lint:teacher
npm run lint:admin

### Build
npm run build:student
npm run build:teacher
npm run build:admin

### Serve (local dev)
npm run serve:student
npm run serve:teacher
npm run serve:admin


### Test
npm run test:student       # Run unit tests
npm run test-fix:student   # Update snapshots


### Cypress (E2E)
```sh
npm run cypress:student


### Storybook (UI Components)
```sh
npm run storybook:student