import {
  AuthResponse,
  CreateProjectRequest,
  CreateProposalRequest,
  CreateReviewRequest,
  FreelancerPortfolioDto,
  LoginRequest,
  ProjectCategoryNode,
  ProjectDto,
  ProjectSearchFilters,
  ProposalDto,
  ProposalCommandResponse,
  ProposalHistoryEntry,
  RegisterRequest,
  ReviewDto,
  UpdateProjectRequest,
  UserDto,
} from '@/types';

const API_BASE = process.env.NEXT_PUBLIC_API_URL || 'http://localhost:5133';

class ApiClient {
  private getToken(): string | null {
    if (typeof window === 'undefined') return null;
    return localStorage.getItem('token');
  }

  private async request<T>(path: string, options: RequestInit = {}): Promise<T> {
    const token = this.getToken();
    const headers: Record<string, string> = {
      'Content-Type': 'application/json',
      ...(options.headers as Record<string, string> || {}),
    };
    if (token) {
      headers['Authorization'] = `Bearer ${token}`;
    }

    const res = await fetch(`${API_BASE}${path}`, {
      ...options,
      headers,
    });

    if (!res.ok) {
      const error = await res.json().catch(() => null);
      const msg = error?.message || error?.Message || res.statusText;
      throw new Error(msg || `HTTP ${res.status}`);
    }

    if (res.status === 204) return undefined as T;
    return res.json();
  }

  // Auth
  register(data: RegisterRequest) {
    return this.request<AuthResponse>('/api/auth/register', {
      method: 'POST',
      body: JSON.stringify(data),
    });
  }

  login(data: LoginRequest) {
    return this.request<AuthResponse>('/api/auth/login', {
      method: 'POST',
      body: JSON.stringify(data),
    });
  }

  // Users
  getMe() {
    return this.request<UserDto>('/api/users/me');
  }

  getUser(id: string) {
    return this.request<UserDto>(`/api/users/${id}`);
  }

  getUserReviews(id: string) {
    return this.request<ReviewDto[]>(`/api/users/${id}/reviews`);
  }

  getFreelancerPortfolio(id: string) {
    return this.request<FreelancerPortfolioDto>(`/api/users/${id}/portfolio`);
  }

  // Projects
  getProjects(filters: ProjectSearchFilters = {}) {
    const params = new URLSearchParams();
    if (filters.status) params.set('status', filters.status);
    if (typeof filters.maxBudget === 'number') params.set('maxBudget', filters.maxBudget.toString());
    if (typeof filters.minBudget === 'number') params.set('minBudget', filters.minBudget.toString());
    if (filters.keyword) params.set('keyword', filters.keyword);
    if (filters.skills && filters.skills.length > 0) params.set('skills', filters.skills.join(','));
    if (filters.type) params.set('type', filters.type);
    if (filters.sortBy) params.set('sortBy', filters.sortBy);

    const qs = params.toString();
    return this.request<ProjectDto[]>(`/api/projects${qs ? `?${qs}` : ''}`);
  }

  getProject(id: string) {
    return this.request<ProjectDto>(`/api/projects/${id}`);
  }

  getProjectCategoriesTree() {
    return this.request<ProjectCategoryNode[]>('/api/projects/categories/tree');
  }

  createProject(data: CreateProjectRequest) {
    return this.request<ProjectDto>('/api/projects', {
      method: 'POST',
      body: JSON.stringify(data),
    });
  }

  createProjectFromTemplate(templateId: string) {
    return this.request<ProjectDto>('/api/projects/from-template', {
      method: 'POST',
      body: JSON.stringify({ templateId }),
    });
  }

  updateProject(id: string, data: UpdateProjectRequest) {
    return this.request<ProjectDto>(`/api/projects/${id}`, {
      method: 'PUT',
      body: JSON.stringify(data),
    });
  }

  assignFreelancer(projectId: string, freelancerId: string) {
    return this.request<void>(`/api/projects/${projectId}/assign`, {
      method: 'PATCH',
      body: JSON.stringify({ freelancerId }),
    });
  }

  completeProject(id: string) {
    return this.request<void>(`/api/projects/${id}/complete`, {
      method: 'PATCH',
    });
  }

  deleteProject(id: string) {
    return this.request<void>(`/api/projects/${id}`, {
      method: 'DELETE',
    });
  }

  // Proposals
  getProjectProposals(projectId: string) {
    return this.request<ProposalDto[]>(`/api/projects/${projectId}/proposals`);
  }

  createProposal(projectId: string, data: CreateProposalRequest) {
    return this.request<ProposalDto>(`/api/projects/${projectId}/proposals`, {
      method: 'POST',
      body: JSON.stringify(data),
    });
  }

  acceptProposal(id: string) {
    return this.request<ProposalCommandResponse>(`/api/proposals/${id}/accept`, { method: 'PATCH' });
  }

  rejectProposal(id: string) {
    return this.request<ProposalCommandResponse>(`/api/proposals/${id}/reject`, { method: 'PATCH' });
  }

  undoProposalCommand(commandId: string) {
    return this.request<void>(`/api/proposals/commands/${commandId}`, { method: 'DELETE' });
  }

  getProposalHistory(projectId: string) {
    return this.request<ProposalHistoryEntry[]>(`/api/projects/${projectId}/proposal-history`);
  }

  // Reviews
  createReview(data: CreateReviewRequest) {
    return this.request<ReviewDto>('/api/reviews', {
      method: 'POST',
      body: JSON.stringify(data),
    });
  }
}

export const apiClient = new ApiClient();
