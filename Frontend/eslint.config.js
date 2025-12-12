// @ts-check
const eslint = require("@eslint/js");
const tseslint = require("typescript-eslint");
const angular = require("angular-eslint");

module.exports = tseslint.config(
  {
    files: ["**/*.ts"],
    extends: [
      eslint.configs.recommended,
      ...tseslint.configs.recommended,
      ...tseslint.configs.stylistic,
      ...angular.configs.tsRecommended,
    ],
    processor: angular.processInlineTemplates,
    rules: {
      "prefer-template": "error",
      "@angular-eslint/prefer-inject": "off",
      "@typescript-eslint/no-explicit-any": "off",
      "@typescript-eslint/explicit-function-return-type": [
        "error",
        {
          allowExpressions: false,
          allowTypedFunctionExpressions: true,
          allowHigherOrderFunctions: true,
          allowDirectConstAssertionInArrowFunctions: true,
        },
      ],
      "@angular-eslint/directive-selector": [
        "error",
        {
          type: "attribute",
          prefix: "app",
          style: "camelCase",
        },
      ],
      "@angular-eslint/component-selector": [
        "error",
        {
          type: "element",
          prefix: "app",
          style: "kebab-case",
        },
      ],
      "func-call-spacing": ["error", "never"],
      "no-array-constructor": "off",
      "no-param-reassign": "off",
      "no-unused-expressions": "off",
      "no-use-before-define": "off",
      "no-useless-constructor": "off",
      quotes: ["error", "single"],
      "max-len": [
        "error",
        {
          code: 180,
        },
      ],
      "max-lines": [
        "error",
        {
          max: 600,
          skipBlankLines: true,
          skipComments: true,
        },
      ],
      curly: "error",
      "no-debugger": "error",
      "no-console": [
        "error",
        {
          allow: ["warn", "error", "dir", "assert"],
        },
      ],
      "no-eval": "error",
      "max-lines-per-function": [
        "error",
        {
          max: 75,
          skipBlankLines: true,
          skipComments: true,
        },
      ],
      camelcase: [
        "error",
        {
          properties: "always",
        },
      ],
      "no-alert": "error",
      eqeqeq: ["error", "always"],
      "no-dupe-class-members": "error",
      "no-dupe-else-if": "error",
      "no-duplicate-case": "error",
      "no-template-curly-in-string": "error",
      "no-this-before-super": "error",
      "valid-typeof": "error",
      "consistent-return": "error",
      "capitalized-comments": ["error", "always"],
      "logical-assignment-operators": ["error", "always"],
    },
  },
  {
    files: ["**/*.html"],
    extends: [
      ...angular.configs.templateRecommended,
      ...angular.configs.templateAccessibility,
    ],
    rules: {},
  }
);
