// ─── Enums ───
export type UserRole = 'Customer' | 'Freelancer' | 'Admin';
export type ProjectStatus = 'Open' | 'InProgress' | 'Completed' | 'Cancelled';
export type ProposalStatus = 'Pending' | 'Accepted' | 'Rejected';
export type ProjectType = 'FixedPrice' | 'Hourly' | 'LongTerm';

// ─── DTOs ───
export interface UserDto {
  id: string;
  email: string;
  name: string;
  role: UserRole;
  createdAt: string;
}

export interface AuthResponse {
  token: string;
  user: UserDto;
}

export interface ProjectDto {
  id: string;
  title: string;
  description: string;
  budget: number;
  status: ProjectStatus;
  type: ProjectType;
  customerId: string;
  customerName: string;
  freelancerId?: string;
  freelancerName?: string;
  createdAt: string;
  deadline?: string;
  requiredSkills: string[];
  proposalCount: number;
}

export interface ProposalDto {
  id: string;
  projectId: string;
  freelancerId: string;
  freelancerName: string;
  coverLetter: string;
  bidAmount: number;
  status: ProposalStatus;
  createdAt: string;
}

export interface ReviewDto {
  id: string;
  fromUserId: string;
  fromUserName: string;
  toUserId: string;
  rating: number;
  comment: string;
  createdAt: string;
}

export interface FreelancerPortfolioDto {
  userId: string;
  displayName: string;
  bio: string;
  avatarUrl: string;
  skills: string[];
  completedProjects: number;
  externalRating: number;
  source: string;
}

export interface ProjectCategoryNode {
  name: string;
  isLeaf: boolean;
  children: ProjectCategoryNode[];
}

export interface ProjectSearchFilters {
  status?: ProjectStatus;
  maxBudget?: number;
  minBudget?: number;
  keyword?: string;
  skills?: string[];
  type?: ProjectType;
  sortBy?: 'newest' | 'budget_asc' | 'budget_desc' | 'deadline';
}

export interface ProposalCommandResponse {
  commandId: string;
}

export interface ProposalHistoryEntry {
  commandId: string;
  commandType: 'AcceptProposal' | 'RejectProposal' | string;
  proposalId: string;
  executedByUserId: string;
  executedAt: string;
}

// ─── Requests ───
export interface RegisterRequest {
  email: string;
  password: string;
  name: string;
  role: UserRole;
}

export interface LoginRequest {
  email: string;
  password: string;
}

export interface CreateProjectRequest {
  title: string;
  description: string;
  budget: number;
  type: ProjectType;
  deadline?: string;
  requiredSkills: string[];
}

export interface UpdateProjectRequest {
  title: string;
  description: string;
  budget: number;
  deadline?: string;
  requiredSkills: string[];
}

export interface CreateProposalRequest {
  coverLetter: string;
  bidAmount: number;
}

export interface CreateReviewRequest {
  toUserId: string;
  rating: number;
  comment: string;
}
