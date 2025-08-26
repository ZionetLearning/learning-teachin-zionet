import type { Config } from "jest";

const config: Config = {
  // Automatically clear mock calls, instances, contexts and results before every test
  clearMocks: true,

  // Indicates which provider should be used to instrument code for coverage
  coverageProvider: "v8",

  // An array of file extensions your modules use
  moduleFileExtensions: ["ts", "tsx", "js", "jsx", "json"],

  // A map from regular expressions to module names or to arrays of module names that allow to stub out resources with a single module
  moduleNameMapper: {
    "^(\\.{1,2}/.*)\\.js$": "$1",
    "^@/(.*)$": "<rootDir>/src/$1",
    "\\.(gif|ttf|eot|svg|png)$":
      "<rootDir>/src/features/practice/speaking-practice/__tests__/__mocks__/fileMock.js",
  },

  // A preset that is used as a base for Jest's configuration
  preset: "ts-jest/presets/default-esm",

  // A list of paths to directories that Jest should use to search for files in
  roots: ["<rootDir>/src/features/practice/speaking-practice/__tests__"],

  // A list of paths to modules that run some code to configure or set up the testing framework before each test
  setupFilesAfterEnv: ["<rootDir>/jest.setup.ts"],

  // The test environment that will be used for testing
  testEnvironment: "jsdom",

  extensionsToTreatAsEsm: [".ts", ".tsx"],

  // An array of regexp pattern strings that are matched against all test paths, matched tests are skipped
  testPathIgnorePatterns: [
    "<rootDir>/src/features/practice/speaking-practice/__tests__/__mocks__/",
  ],

  // A map from regular expressions to paths to transformers
  transform: {
    "^.+\\.tsx?$": [
      "ts-jest",
      {
        diagnostics: {
          ignoreCodes: [1343],
        },
        astTransformers: {
          before: [
            {
              path: "ts-jest-mock-import-meta",
              options: {
                metaObjectReplacement: {
                  env: {
                    VITE_AZURE_SPEECH_KEY: "VITE_AZURE_SPEECH_KEY",
                    VITE_AZURE_REGION: "VITE_AZURE_REGION",
                  },
                },
              },
            },
          ],
        },
      },
    ],
  },

  // An array of regexp pattern strings that are matched against all source file paths, matched files will skip transformation
  transformIgnorePatterns: ["/node_modules/"],
};

export default config;
