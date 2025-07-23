import type { MessageSender } from "../types";

export const AI_SENDER: MessageSender = {
  id: "ai-assistant",
  name: "AI Assistant",
  type: "ai",
  avatar: "🤖",
};

export const USER_SENDER: MessageSender = {
  id: "user-1",
  name: "You",
  type: "user",
  avatar: "👤",
};

export const TEXT_RESPONSE_TEMPLATES = [
  "That's interesting! Regarding \"{userMessage}\", here's what I think:",
  'Great question about "{userMessage}". Let me share some insights:',
  'Thanks for asking! When it comes to "{userMessage}", I\'d say:',
];

export const TEXT_FOLLOWUP_MESSAGES = [
  "This is a complex topic with many aspects to explore.",
  "There are several ways to approach this. What interests you most?",
  "I'd be happy to dive deeper into any specific part you're curious about.",
];

export const IMAGE_CAPTIONS = [
  "Here's an image that relates to your request!",
  "I found this visual that might help illustrate the concept.",
  "This image should provide some context for what you're asking about.",
];


export const MOCK_QUIZZES = [
  {
    question: "What's the largest planet in our solar system?",
    options: ["Earth", "Jupiter", "Saturn", "Neptune"],
    correctAnswer: 1,
    explanation: "Jupiter is the largest planet in our solar system.",
  },
  {
    question: "What does HTML stand for?",
    options: [
      "Hypertext Markup Language",
      "High Tech Modern Language",
      "Home Tool Markup Language",
      "Hyperlink Text Markup Language",
    ],
    correctAnswer: 0,
    explanation: "HTML stands for Hypertext Markup Language.",
  },
  {
    question: "Which company developed React?",
    options: ["Google", "Microsoft", "Facebook", "Apple"],
    correctAnswer: 2,
    explanation: "React was developed by Facebook (now Meta).",
  },
];

// Link data
export const MOCK_LINKS = [
  {
    title: "Home Page",
    description: "Return to the main application homepage",
    url: "/",
    buttonText: "Go Home",
  },
  {
    title: "Avatar Features",
    description: "You can interact with avatars here",
    url: "/avatar/ou",
    buttonText: "Speak with an Avatar",
  },
  {
    title: "Another Chat App",
    description: "Browse other chat applications",
    url: "/chat/da",
    buttonText: "Read Docs",
  },
];

// Initial conversation data
export const INITIAL_CONVERSATION_CONFIG = {
  id: "demo-conversation",
  title: "Smart Chat Demo - All Message Types Showcase",
  welcomeMessage: {
    id: "msg-welcome",
    type: "text" as const,
    content:
      "🎯 Welcome to the Smart Chat Demo! Try typing messages like '*show me an image*', '*create a quiz*', or '*give me a link*' to see different response types!",
    timestampOffset: 300000, // 5 minutes ago
  },
};

// Keyword detection patterns
export const MESSAGE_PATTERNS = {
  image: ["image", "picture", "photo"],
  quiz: ["quiz", "question", "test"],
  link: ["link", "page", "navigate"],
};

// Timing configurations
export const TIMING_CONFIG = {
  networkDelay: {
    min: 500,
    max: 1500,
  },
  typingDelay: {
    min: 1000,
    max: 2500,
  },
};
