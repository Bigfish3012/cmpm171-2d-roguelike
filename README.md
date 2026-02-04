# CMPM171 2D Roguelike

A 2D Roguelike game project developed with Unity.
- **Engine**: Unity 6000.2.7f2
- **Genre**: 2D Roguelike
## Collaboration Workflow

### 1. Clone Repository
```bash
git clone git@github.com:Bigfish3012/cmpm171-2d-roguelike.git
```

### 2. Create Your Own Branch
```bash
# Create and switch to a new branch from main
# Change "your-branch-name" to a name that you like to call
# Make sure that you are working on your own branch
# don't merge any change to the main branch before testing
git checkout -b your-branch-name 
```

### 3. Work on Your Branch
- Develop and make changes on your branch
- Commit changes regularly:
```bash
git add .
// // or you can use "git add <file name>", "git add <file name> <file name>"
git commit -m "Describe your changes"
```

### 4. Push to **test-main** Branch
After completing your work, push your branch to the remote repository and create a Pull Request to merge into the `test-main` branch:
```bash
# Push your branch to the remote repository
git push origin your-branch-name

# If you see an error about no upstream branch, set the upstream:
git push --set-upstream origin your-branch-name

# Create a Pull Request on GitHub/GitLab, targeting the test-main branch
```

**Note**: If you encounter the error "The current branch has no upstream branch", use `git push --set-upstream origin your-branch-name` to set the remote tracking branch.

### 5. Merge to main Branch
- Test and verify all features on the `test-main` branch
- Once everything is confirmed, create a Pull Request to merge `test-main` into the `main` branch
- Only thoroughly tested and reviewed code should be merged into the `main` branch

### Notes
- Always create your working branch from the latest `main` or `test-main` branch
- Ensure your code compiles and runs correctly before committing
- Write clear and descriptive commit messages
- Regularly sync updates from the main branches to your branch:
```bash
git checkout your-branch-name
git merge test-main
```

## Project Overview

This project is a 2D Roguelike game featuring procedurally generated maps and random elements, providing players with a unique experience each time they play.

## Branch Strategy

This is a team project with the following branches:

- **main** - Main production branch
- **test-main** - Testing branch for all game features that need to be deployed
